using Microsoft.VisualStudio.TestTools.UnitTesting;
using VMATTBIautoPlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMATTBIautoPlan.Tests
{
    [TestClass()]
    public class IsoNameHelperTests
    {
        static readonly string Head = "Head";
        static readonly string Thorax = "Thorax";
        static readonly string Abdomen = "Abdomen";
        static readonly string Pelvis = "Pelvis";
        static readonly string Legs = "Legs";
        static readonly string Legs_Sup = "Legs_Sup";
        static readonly string Legs_Inf = "Legs_Inf";
        static readonly string Feet = "Feet";

        #region Iso Name True Tests
        [TestMethod()]
        public void IsTopTest_Head()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Head));
        }

        [TestMethod()]
        public void IsTopTest_Thorax()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Thorax));
        }

        [TestMethod()]
        public void IsTopTest_Abdomen()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Abdomen));
        }
        [TestMethod()]
        public void IsTopTest_Pelvis()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Pelvis));
        }



        [TestMethod()]
        public void IsNotBottomTest_Head()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Thorax));
        }
        [TestMethod()]
        public void IsNotBottomTest_Thorax()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Thorax));
        }
        [TestMethod()]
        public void IsNotBottomTest_Abdomen()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Abdomen));
        }
        [TestMethod()]
        public void IsNotBottomTest_Pelvis()
        {
            Assert.IsTrue(IsoNameHelper.IsTop(Pelvis));
        }

        #endregion

        #region Iso Name False Tests

        [TestMethod()]
        public void IsNotTop_Legs()
        {
            Assert.IsFalse(IsoNameHelper.IsTop(Legs));
        }

        [TestMethod()]
        public void IsNotTop_Legs_Sup()
        {
            Assert.IsFalse(IsoNameHelper.IsTop(Legs_Sup));
        }

        [TestMethod()]
        public void IsNotTop_Legs_Inf()
        {
            Assert.IsFalse(IsoNameHelper.IsTop(Legs_Inf));
        }

        [TestMethod()]
        public void IsNotTop_Feet()
        {
            Assert.IsFalse(IsoNameHelper.IsTop(Feet));
        }

        [TestMethod()]
        public void IsBottomTest_Legs()
        {
            Assert.IsTrue(IsoNameHelper.IsBottom(Legs));
        }
        [TestMethod()]
        public void IsBottomTest_Legs_Sup()
        {
            Assert.IsTrue(IsoNameHelper.IsBottom(Legs_Sup));
        }
        [TestMethod()]
        public void IsBottomTest_Legs_Inf()
        {
            Assert.IsTrue(IsoNameHelper.IsBottom(Legs_Inf));
        }
        [TestMethod()]
        public void IsBottomTest_Feet()
        {
            Assert.IsTrue(IsoNameHelper.IsBottom(Feet));
        }

        #endregion


        #region Head First-Supine Tests

        [TestMethod()]
        public void OrientationTest_Head()
        {
            Assert.AreEqual("Head First-Supine", IsoNameHelper.Orientation(Head));
        }

        [TestMethod()]
        public void OrientationTest_Thorax()
        {
            Assert.AreEqual("Head First-Supine", IsoNameHelper.Orientation(Thorax));
        }
        [TestMethod()]
        public void OrientationTest_Abdomen()
        {
            Assert.AreEqual("Head First-Supine", IsoNameHelper.Orientation(Abdomen));
        }
        [TestMethod()]
        public void OrientationTest_Pelvis()
        {
            Assert.AreEqual("Head First-Supine", IsoNameHelper.Orientation(Pelvis));
        }

        #endregion

        #region Feet First-Supine Tests
        [TestMethod()]
        public void OrientationTest_Legs()
        {
            Assert.AreEqual("Feet First-Supine", IsoNameHelper.Orientation(Legs));
        }
        [TestMethod()]
        public void OrientationTest_Legs_Sup()
        {
            Assert.AreEqual("Feet First-Supine", IsoNameHelper.Orientation(Legs_Sup));
        }
        [TestMethod()]
        public void OrientationTest_Legs_Inf()
        {
            Assert.AreEqual("Feet First-Supine", IsoNameHelper.Orientation(Legs_Inf));
        }
        [TestMethod()]
        public void OrientationTest_Feet()
        {
            Assert.AreEqual("Feet First-Supine", IsoNameHelper.Orientation(Feet));
        }


        #endregion


        [TestMethod()]
        public void OrientationTest_Invalid()
        {
            Assert.AreEqual("Unknown", IsoNameHelper.Orientation("Invalid"));
        }

    }
}