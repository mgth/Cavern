using Cavern.Utilities;

namespace Test.Cavern {
    /// <summary>
    /// Tests the <see cref="WaveformUtils"/> class.
    /// </summary>
    [TestClass]
    public class WaveformUtils_Tests {
        /// <summary>
        /// Tests if <see cref="WaveformUtils.TrimEnd(float[][])"/> correctly cuts the end of jagged arrays.
        /// </summary>
        [TestMethod, Timeout(1000)]
        public void TrimEnd_2D() {
            float[][] source = {
                new float[100], // Will be cut until the nicest element
                new float[100], // Will be empty, but not cut, since the other jagged array is longer
            };
            source[0][Consts.nice] = 1;
            WaveformUtils.TrimEnd(source);

            Assert.AreEqual(source[0].Length, Consts.nice + 1);
            Assert.AreEqual(source[0].Length, source[1].Length);
        }
    }
}