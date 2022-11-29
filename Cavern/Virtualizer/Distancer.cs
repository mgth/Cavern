﻿using System;
using System.Numerics;

using Cavern.Filters;
using Cavern.Utilities;

namespace Cavern.Virtualizer {
    /// <summary>
    /// Handles distancing calculations for a single source's two ears.
    /// </summary>
    public class Distancer {
        /// <summary>
        /// The left ear's gain that corresponds to the <see cref="source"/>'s distance.
        /// </summary>
        public float LeftGain { get; private set; }

        /// <summary>
        /// The left ear's gain that corresponds to the <see cref="source"/>'s distance.
        /// </summary>
        public float RightGain { get; private set; }

        /// <summary>
        /// Decreases real distances by this factor to shrink the environment's scale.
        /// </summary>
        public float distanceFactor;

        /// <summary>
        /// The filtered source.
        /// </summary>
        readonly Source source;

        /// <summary>
        /// The filter processing the <see cref="source"/>.
        /// </summary>
        readonly SpikeConvolver filter;

        /// <summary>
        /// The maximum length of any of the <see cref="impulses"/>, because if the <see cref="FastConvolver"/> is used,
        /// the arrays won't be reassigned and the filter won't cut out, and if the <see cref="SpikeConvolver"/> is used,
        /// the overhead is basically zero.
        /// </summary>
        readonly int filterSize;

        /// <summary>
        /// Create a distance simulation for a <see cref="Source"/>.
        /// </summary>
        public Distancer(Source source) {
            this.source = source;

            // Add the delays to the impulses that were removed for storage optimization
            if (impulseDelays[0][0] != 0) {
                for (int i = 0; i < impulses.Length; ++i) {
                    for (int j = 0; j < impulses[i].Length; ++j) {
                        int convLength = impulses[i][j].Length;
                        short delay = impulseDelays[i][j];
                        Array.Resize(ref impulses[i][j], convLength + delay);
                        Array.Copy(impulses[i][j], 0, impulses[i][j], delay, convLength);
                        Array.Clear(impulses[i][j], 0, convLength);
                    }
                }
                impulseDelays[0][0] = 0;
            }

            source.VolumeRolloff = Rolloffs.Disabled;
            for (int i = 0; i < impulses.Length; ++i) {
                for (int j = 0; j < impulses[i].Length; ++j) {
                    if (filterSize < impulses[i][j].Length) {
                        filterSize = impulses[i][j].Length;
                    }
                }
            }
            distanceFactor = Math.Max(Math.Max(Listener.EnvironmentSize.X, Listener.EnvironmentSize.Y), Listener.EnvironmentSize.Z);
            filter = new SpikeConvolver(new float[filterSize], 0);
        }

        /// <summary>
        /// Generate the left/right ear filters.
        /// </summary>
        /// <param name="right">The object is to the right of the <see cref="Listener"/>'s forward vector</param>
        /// <param name="samples">Single-channel downmixed samples to process</param>
        public void Generate(bool right, float[] samples) {
            float dirMul = -90;
            if (right) {
                dirMul = 90;
            }
            Vector3 sourceForward = new Vector3(0, dirMul, 0).RotateInverse(source.listener.Rotation).PlaceInSphere(),
                dir = source.Position - source.listener.Position;
            float distance = dir.Length(),
                rawAngle = (float)Math.Acos(Vector3.Dot(sourceForward, dir) / distance),
                angle = rawAngle * VectorExtensions.Rad2Deg;
            distance /= distanceFactor;

            // Find bounding angles with discrete impulses
            int smallerAngle = 0;
            while (smallerAngle < angles.Length && angles[smallerAngle] < angle) {
                ++smallerAngle;
            }
            if (smallerAngle != 0) {
                --smallerAngle;
            }
            int largerAngle = smallerAngle + 1;
            if (largerAngle == angles.Length) {
                largerAngle = angles.Length - 1;
            }
            float angleRatio = Math.Min(QMath.LerpInverse(angles[smallerAngle], angles[largerAngle], angle), 1);

            // Find bounding distances with discrete impulses
            int smallerDistance = 0;
            while (smallerDistance < distances.Length && distances[smallerDistance] < distance) {
                ++smallerDistance;
            }
            if (smallerDistance != 0) {
                --smallerDistance;
            }
            int largerDistance = smallerDistance + 1;
            if (largerDistance == distances.Length)
                largerDistance = distances.Length - 1;
            float distanceRatio =
                Math.Clamp(QMath.LerpInverse(distances[smallerDistance], distances[largerDistance], distance), 0, 1);

            // Find impulse candidates and their weight
            float[][] candidates = new float[4][] {
                impulses[smallerAngle][smallerDistance],
                impulses[smallerAngle][largerDistance],
                impulses[largerAngle][smallerDistance],
                impulses[largerAngle][largerDistance]
            };
            float[] gains = new float[4] {
                (float)Math.Sqrt((1 - angleRatio) * (1 - distanceRatio)),
                (float)Math.Sqrt((1 - angleRatio) * distanceRatio),
                (float)Math.Sqrt(angleRatio * (1 - distanceRatio)),
                (float)Math.Sqrt(angleRatio * distanceRatio)
            };

            // Apply the ear canal's response
            Array.Clear(filter.Impulse, 0, filterSize);
            for (int candidate = 0; candidate < candidates.Length; ++candidate) {
                WaveformUtils.Mix(candidates[candidate], filter.Impulse, gains[candidate]);
            }
            filter.Process(samples);

            // Apply gains
            float angleDiff = (float)(Math.Sin(rawAngle) * .097f);
            float ratioDiff = (distance + angleDiff) * (VirtualizerFilter.referenceDistance - angleDiff) /
                             ((distance - angleDiff) * (VirtualizerFilter.referenceDistance + angleDiff));
            ratioDiff *= ratioDiff;
            if (right) {
                if (ratioDiff < 1) {
                    RightGain = ratioDiff;
                } else {
                    LeftGain = 1 / ratioDiff;
                }
            } else {
                if (ratioDiff < 1) {
                    LeftGain = ratioDiff;
                } else {
                    RightGain = 1 / ratioDiff;
                }
            }
        }

        /// <summary>
        /// All the angles that have their own impulse responses.
        /// </summary>
        static readonly float[] angles = { 0, 15, 30, 45, 60, 75, 90 };

        /// <summary>
        /// All the distances that have their own impulse responses for each angle in meters.
        /// </summary>
        static readonly float[] distances = { .1f, .25f, .5f, 1, 2 };

        /// <summary>
        /// Ear canal distortion impulse responses for given angles and distances. The first dimension is the angle,
        /// provided in <see cref="angles"/>, and the second dimension is the distance, provided in <see cref="distances"/>.
        /// The delays for each filter are found in <see cref="impulseDelays"/> and are applied when running the constructor first.
        /// </summary>
        static readonly float[][][] impulses = {
            new float[5][] {
                new float[] { .07280911f, .06462494f, .03621706f, .02388191f, 1, .3592792f, .01247505f },
                new float[] { .1076947f, 1, .7025662f, .4730703f, .6746233f, .4077276f, .1590533f, .007440981f },
                new float[] { 1, .9599667f, .7574043f, .563445f, .384205f, .205583f, .04723928f },
                new float[] { .3099651f, 1, .8210505f, .6432168f, .4701469f, .314512f, .1522951f, .01557517f },
                new float[] { 1, .995011f, .8049579f, .6124563f, .4361598f, .2638117f, .08705958f }
            },
            new float[5][] {
                new float[] { .1344281f, 1, .3292527f, .1896656f, .1199801f, .06613702f, .02265457f, .0001127756f },
                new float[] { .01539751f, 1, .4081665f, .1379157f, .2765296f, .004177992f, .001445532f, 8.536115E-06f },
                new float[] { .003450079f, 1, .3951304f, .3269113f, .0693358f, .1523703f, .07541669f, .0004151615f },
                new float[] { 1, .6661633f, .3091478f, .2058491f, .1283903f, .0767751f, .0009242403f, 2.944942E-05f },
                new float[] { .2499741f, 1, .3774478f, .3133463f, .1880997f, .1250631f, .0623516f, .0002920433f }
            },
            new float[5][] {
                new float[] { .06257993f, .09523416f, .5931237f, .1156644f, 1, .005021416f, .00050077f },
                new float[] { .2355082f, 1, .5970099f, .3400322f, .203131f, .1150175f, .04886235f, .003074998f },
                new float[] { 1, .9814389f, .7342445f, .5150265f, .3413402f, .1660449f, .02730976f },
                new float[] { .6118575f, 1, .8684458f, .6847231f, .4924142f, .3063608f, .126405f, .005869763f },
                new float[] { .1565535f, 1, .813062f, .6518386f, .4835832f, .3537659f, .1754839f, .0338771f }
            },
            new float[5][] {
                new float[] { .009142146f, 1, .2878403f, .08227473f, .003490633f, .001465153f, .0001208418f },
                new float[] { .6204942f, 1, .6063665f, .3632298f, .2120882f, .1125071f, .03606193f, .0008710854f },
                new float[] { .371588f, 1, .8135141f, .6742395f, .4747304f, .2724313f, .1150636f, .008209253f },
                new float[] { .2152367f, .5590446f, 1, .9919566f, .8057337f, .5330636f, .2248113f, .01110894f },
                new float[] { .5535611f, 1, .8372242f, .6353171f, .4756832f, .3067128f, .1420829f, .01671888f }
            },
            new float[5][] {
                new float[] { .7265214f, 1, .5024924f, .2297902f, .1199771f, .03201693f, .0005188733f },
                new float[] { .1858565f, 1, .6881534f, .4469453f, .2566884f, .1449862f, .06030752f, .009879692f },
                new float[] { .07542851f, .9329132f, 1, .7792008f, .5683066f, .3888762f, .2322347f, .08083527f, .009146959f },
                new float[] { 1, .03538768f, .03892107f, .03323091f, .02448232f, .01700315f, .01041788f, .003600323f },
                new float[] { .4779726f, 1, .9291803f, .6367276f, .5111281f, .3458712f, .1375917f, .01566455f }
            },
            new float[5][] {
                new float[] { .8114715f, 1, .4208195f, .2168841f, .04628683f, .001293332f },
                new float[] { .6006778f, 1, .5405784f, .3098713f, .2071735f, .4052377f, .0399457f, .007321218f, 7.012694E-05f },
                new float[] { .1129211f, 1, .1718559f, .1381525f, .1818777f, .06429848f, .03662953f, .01753195f, .00486109f, 4.179159E-05f },
                new float[] { .7197832f, 1, .1481909f, .1208788f, .08932336f, .05708343f, .03211127f, .01443417f, .002165545f },
                new float[] { .09626135f, 1, .9233426f, .8441116f, .5399653f, .4045798f, .2599762f, .1126483f, .02108685f }
            },
            new float[5][] {
                new float[] { .004066892f, 1, .9276068f, .689859f, .6293351f, .3191521f, .215393f, .4407475f, .06752289f, .02275242f, .002159857f, 5.570841E-05f },
                new float[] { .1883319f, 1, .3328354f, .2746456f, .7198558f, .2774538f, .2316711f, .2145967f, .09675916f, .0682244f, .05688404f, .05102973f, .03165466f, .02654572f, .01556074f, .006934607f, .0006434356f },
                new float[] { .03379108f, .4993314f, .2378534f, .3391335f, .1794211f, .1522885f, .3848652f, 1, .3525296f, .09647691f, .4420892f, .1903519f, .06009025f, .04461118f, .1563454f, .1516566f, .03111456f, .02811026f, .02422654f, .01810599f, .0170227f, .01517106f, .01244155f, .008510697f, .007499579f, .003624003f, .002449588f, .0003752929f },
                new float[] { 4.703207E-05f, 0, 1, .169429f, .1465525f, .1309407f, .1184505f, .09640431f, .08442533f, .07750326f, .06277364f, .0565761f, .04843304f, .03912108f, .02593273f, .02114692f, .009919425f, .005272163f, .002747248f, .002523138f, .0007947971f, 8.330223E-05f, 4.133264E-05f },
                new float[] { .09626135f, .9999999f, .9233426f, .8441116f, .5399653f, .4045798f, .2599762f, .1126483f, .02108685f }
            }
        };

        /// <summary>
        /// Trailing zeros to add before the <see cref="impulses"/>.
        /// </summary>
        static readonly short[][] impulseDelays = {
            new short[] { 8, 28, 63, 132, 271 },
            new short[] { 7, 28, 64, 132, 270 },
            new short[] { 7, 27, 62, 131, 269 },
            new short[] { 6, 26, 60, 130, 268 },
            new short[] { 5, 24, 58, 128, 267 },
            new short[] { 4, 23, 58, 129, 268 },
            new short[] { 0, 22, 61, 141, 268 }
        };
    }
}