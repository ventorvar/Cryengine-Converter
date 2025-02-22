﻿using CgfConverter;
using CgfConverterTests.TestUtilities;
using grendgine_collada;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CgfConverterTests.IntegrationTests.MWO
{
    [TestClass]
    public class MWOIntegrationTests
    {
        private readonly TestUtils testUtils = new TestUtils();
        string userHome;

        [TestInitialize]
        public void Initialize()
        {
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;
            userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            testUtils.GetSchemaSet();
        }

        [TestMethod]
        public void MWO_industrial_wetlamp_a_MaterialFileNotFound()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\industrial_wetlamp_a.cgf", "-dds", "-dae", "-objectdir", @"d:\depot\mwo\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(3, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_timberwolf_chr()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\timberwolf.chr", "-dds", "-dae", "-objectdir", @"d:\depot\lol\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            colladaData.GenerateDaeObject();

            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(11, actualMaterialsCount);
            
            // Visual Scene Check 
            Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
            Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
            Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
            Assert.AreEqual(2, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);

            // Armature Node check
            var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
            Assert.AreEqual("Armature", node.ID);
            Assert.AreEqual("Bip01", node.sID);
            Assert.AreEqual("Bip01", node.Name);
            Assert.AreEqual("JOINT", node.Type.ToString());
            Assert.AreEqual("-0 -1 0 0 1 -0 0 0 0 0 1 0 0 0 0 1", node.Matrix[0].Value_As_String);
            var pelvisNode = node.node[0];
            Assert.AreEqual("Armature_Bip01_Pelvis", pelvisNode.ID);
            Assert.AreEqual("Bip01_Pelvis", pelvisNode.Name);
            Assert.AreEqual("Bip01_Pelvis", pelvisNode.sID);
            Assert.AreEqual("JOINT", pelvisNode.Type.ToString());
            Assert.AreEqual("0 1 0 0 -0 -0 1 -0.000001 1 -0 0 8.346858 0 0 0 1", pelvisNode.Matrix[0].Value_As_String);
            Assert.AreEqual(3, pelvisNode.node.Length);
            var pitchNode = pelvisNode.node.Where(a => a.ID == "Armature_Bip01_Pitch").FirstOrDefault();
            var leftHipNode = pelvisNode.node.Where(a => a.ID == "Armature_Bip01_L_Hip").FirstOrDefault();
            var rightHipNode = pelvisNode.node.Where(a => a.ID == "Armature_Bip01_R_Hip").FirstOrDefault();
            Assert.IsNotNull(pitchNode);
            Assert.AreEqual("Bip01_Pitch", pitchNode.sID);
            Assert.AreEqual("0 0.999753 0.022217 -0.000001 -0 -0.022217 0.999753 0 1 -0 0 1.627025 0 0 0 1", leftHipNode.Matrix[0].Value_As_String);
            Assert.AreEqual("0 -0.999753 0.022217 -0.000002 0 0.022217 0.999753 0 -1 -0 0 -1.627022 0 0 0 1", rightHipNode.Matrix[0].Value_As_String);
            Assert.AreEqual("0.452222 0.891861 0.008838 0.779598 -0.891905 0.452200 0.004481 -0 0 -0.009909 0.999951 0.000001 0 0 0 1", pitchNode.Matrix[0].Value_As_String);
            
            // Geometry Node check
            node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[1];
            Assert.AreEqual("timberwolf.chr", node.ID);
            Assert.AreEqual("timberwolf.chr", node.Name);
            Assert.AreEqual("NODE", node.Type.ToString());
            Assert.IsNull(node.Instance_Geometry);
            Assert.AreEqual(1, node.Instance_Controller.Length);
            Assert.AreEqual("#Armature", node.Instance_Controller[0].Skeleton[0].Value);

            // Controller check
            var controller = daeObject.Library_Controllers.Controller[0];
            Assert.AreEqual("Controller", controller.ID);
            var skin = controller.Skin;
            Assert.AreEqual("#timberwolf-mesh", skin.source);
            Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", skin.Bind_Shape_Matrix.Value_As_String);
            Assert.AreEqual(3, skin.Source.Length);
            var controllerJoints = skin.Source.Where(a => a.ID == "Controller-joints").First();
            var controllerBindPose = skin.Source.Where(a => a.ID == "Controller-bind_poses").First();
            var controllerWeights = skin.Source.Where(a => a.ID == "Controller-weights").First();
            var joints = skin.Joints;
            Assert.AreEqual(64, controllerJoints.Name_Array.Count);
            Assert.AreEqual("Controller-joints-array", controllerJoints.Name_Array.ID);
            var nameArray = controllerJoints.Name_Array.Value();
            Assert.AreEqual(64, nameArray.Count());
            Assert.IsTrue(nameArray.Contains("Bip01"));
            Assert.IsTrue(nameArray.Contains("Bip01_L_Thigh"));
            Assert.IsTrue(nameArray.Contains("Bip01_R_Toe0Nub"));

            var bindPoseArray = "-0 1 0 0 -1 -0 0 0 0 0 1 -0 0 0 0 1 0 0 1 -8.346858 0 1 -0 0.000002 -1 0 0 -0.000001 0 0 0 1 -1 -0 0 -1.627027 0 -0.022216 0.999753 -8.344796 -0 0.999753 0.022216 -0.185437 0 0 0 1 0 -0.891905 0.452222 -4.127187 0.009909 0.452200 0.891861 -8.139535 -0.999951 0.004481 0.008838 -0.080661 0 0 0 1 1 -0 -0 -1.627022 -0 0.022216 -0.999753 8.344796 0 0.999753 0.022216 -0.185438 0 0 0 1 -0.003118 -0.652256 -0.757992 6.317856 0.003048 0.757986 -0.652263 5.453145 0.999990 -0.004344 -0.000376 2.888252 0 0 0 1 0.002098 0.554768 -0.832002 6.014472 0.003823 0.831994 0.554772 -1.264842 0.999990 -0.004344 -0.000376 2.888253 0 0 0 1 0 -0.000001 -1 1.399999 -0 1 -0.000001 -0.599996 1 0 0 2.885148 0 0 0 1 0.999961 -0.008298 -0.003012 2.504233 0.008648 0.989305 0.145603 1.111563 0.001772 -0.145624 0.989338 -5.841687 0 0 0 1 1 0 0 2.885148 -0 1 -0.000001 -0.599996 -0 0.000001 1 0.000002 0 0 0 1 0.256676 0.957929 -0.128409 -0.833228 -0.011007 0.135749 0.990682 -0.751913 0.966435 -0.252871 0.045388 2.090546 0 0 0 1 -0.256595 0.957623 -0.130836 -2.347204 -0.011012 0.132463 0.991127 -0.773801 0.966456 0.255758 -0.023444 3.607112 0 0 0 1 0 -0.972471 -0.233025 -0.390619 0 -0.233025 0.972471 -0.564066 -1 -0 0 -2.885148 0 0 0 1 -0 0.959551 -0.281534 -1.044431 -0.010661 0.281518 0.959497 -0.941531 0.999943 0.003002 0.010229 2.875273 0 0 0 1 0.257573 0.957690 -0.128399 -1.369874 0.012224 0.129642 0.991485 -0.701470 0.966181 -0.256950 0.021686 2.109286 0 0 0 1 0.210433 0.842365 -0.496124 -1.189731 0.011021 0.505411 0.862808 -1.430004 0.977546 -0.187031 0.097072 1.947561 0 0 0 1 0.210433 0.842365 -0.496124 -2.132204 0.011021 0.505411 0.862808 -1.430004 0.977546 -0.187031 0.097072 1.947561 0 0 0 1 -0.257518 0.957376 -0.130827 -2.893456 -0.035051 0.126049 0.991405 -0.863275 0.965638 0.259890 0.001097 3.583981 0 0 0 1 -0.225088 0.836700 -0.499268 -2.485888 -0.031675 0.505863 0.862032 -1.584614 0.973823 0.209848 -0.087362 3.763780 0 0 0 1 -0.225089 0.836700 -0.499268 -3.424760 -0.031675 0.505863 0.862032 -1.584614 0.973823 0.209848 -0.087362 3.763782 0 0 0 1 -0 -0.972471 -0.233025 -1.168555 0 -0.233025 0.972471 -0.564066 -1 0 0 -2.885148 0 0 0 1 -0 0.959551 -0.281534 -2.961370 -0.010661 0.281518 0.959497 -0.941530 0.999943 0.003002 0.010229 2.875274 0 0 0 1 1 0 -0 2.885148 -0 1 0.000001 -3.098014 0 -0.000001 1 0.000009 0 0 0 1 0 -0 1 -9.282160 -0 1 0 0.307090 -1 -0 0 0 0 0 0 1 0.000001 -0 1 -10.726149 -0 1 0 0.307090 -1 -0 0.000001 -0.000005 0 0 0 1 0.000001 -0 1 -10.957759 -0.000001 1 0 0.307090 -1 -0.000001 0.000001 -0.000009 0 0 0 1 -0.531478 -0.000001 -0.847072 8.086329 0 -1 0 -1.700724 -0.847072 -0 0.531478 -10.162396 0 0 0 1 0.531477 0.000001 -0.847073 8.086336 -0 -1 -0.000001 -1.700709 -0.847073 0 -0.531477 10.162374 0 0 0 1 1 0.000001 -0.000001 0.000010 -0.000001 1 0 -2.857098 0.000001 -0 1 -12.291666 0 0 0 1 1 0.000001 -0.000001 0.000009 -0.000001 1 0 -2.857097 0.000001 -0 1 -12.291667 0 0 0 1 -1 -0.000001 0.000001 -1.454218 -0 -0.707107 -0.707107 5.404586 0.000001 -0.707107 0.707107 -8.957491 0 0 0 1 -1 -0.000001 0.000001 -1.710584 -0 -0.707107 -0.707107 5.344195 0.000001 -0.707107 0.707107 -9.317745 0 0 0 1 -1 -0.000001 0.000001 1.511648 -0 -0.707107 -0.707107 5.402447 0.000002 -0.707107 0.707107 -8.954850 0 0 0 1 -1 -0.000001 0.000001 1.767603 -0 -0.707107 -0.707107 5.342055 0.000002 -0.707107 0.707107 -9.315106 0 0 0 1 -1 -0.000001 0.000001 0.041891 -0 -0.707107 -0.707107 5.313381 0.000001 -0.707107 0.707107 -9.133546 0 0 0 1 1 0.000001 -0.000001 0.000006 -0.000001 1 -0 -1.442909 0.000001 0 1 -11.937682 0 0 0 1 1 0.000001 -0.000001 -1.749993 -0.000001 1 0 -0.788977 0.000001 -0 1 -11.937676 0 0 0 1 1 0.000001 -0.000001 1.750005 -0.000001 1 -0 -0.788976 0.000001 0 1 -11.937681 0 0 0 1 -0.000001 -0.016842 -0.999858 11.221415 0.000188 -0.999858 0.016842 -1.889054 -1 -0.000188 0.000004 -4.937831 0 0 0 1 -0.000188 1 -0.000001 1.745410 -0.000004 -0.000001 -1 8.543955 -1 -0.000188 0.000004 -4.937831 0 0 0 1 -0 1 -0.000001 -1.278729 -0.011111 -0.000001 -0.999938 8.488578 -0.999938 -0 0.011111 -5.032661 0 0 0 1 0.999938 0.000191 0.011106 5.302608 -0.000191 1 0 1.515383 -0.011106 -0.000003 0.999938 -8.098288 0 0 0 1 -0 -0.016841 -0.999858 11.221406 -0.000191 -0.999858 0.016841 -1.889019 -1 0.000191 -0.000003 4.937818 0 0 0 1 0.000192 1 0 1.745375 0.000003 0 -1 8.543945 -1 0.000192 -0.000003 4.937819 0 0 0 1 0.000004 1 0.000001 -1.278764 -0.000001 0.000001 -1 8.543962 -1 0.000004 0.000001 4.938029 0 0 0 1 1 -0.000195 0.000003 -4.877823 0.000195 1 0.000002 1.695347 -0.000003 -0.000002 1 -8.533949 0 0 0 1 -0.003118 -0.652256 -0.757993 6.335854 0.003049 0.757987 -0.652263 5.435551 0.999990 -0.004345 -0.000375 -2.881985 0 0 0 1 0.002098 0.554769 -0.832002 6.002367 0.003823 0.831993 0.554773 -1.286910 0.999990 -0.004345 -0.000375 -2.881984 0 0 0 1 0 0 -1 1.400005 0 1 0 -0.600001 1 -0 0 -2.885144 0 0 0 1 0.999961 -0.008299 -0.003011 -3.075839 0.008649 0.989305 0.145604 1.091651 0.001771 -0.145625 0.989338 -5.711909 0 0 0 1 1 -0 -0 -2.885144 0 1 0 -0.600001 0 -0 1 -0.000008 0 0 0 1 -0.256676 0.957929 -0.128408 -0.833235 -0.011016 0.129951 0.991459 -0.704093 0.966435 0.255898 -0.022803 -2.107132 0 0 0 1 0.256594 0.957623 -0.130835 -2.347206 -0.011004 0.138253 0.990336 -0.691449 0.966456 -0.252675 0.046013 -3.623798 0 0 0 1 -0 -0.972471 -0.233025 -0.390614 -0 -0.233025 0.972471 -0.564074 -1 0 -0 2.885144 0 0 0 1 0 0.959551 -0.281533 -1.044434 -0.010661 0.281517 0.959497 -0.880024 0.999943 0.003001 0.010230 -2.894692 0 0 0 1 -0.257574 0.957690 -0.128398 -1.369879 -0.034420 0.123703 0.991722 -0.652829 0.965645 0.259861 0.001101 -2.124844 0 0 0 1 -0.225536 0.838453 -0.496115 -1.156751 -0.031134 0.502773 0.863858 -1.392009 0.973737 0.210277 -0.087289 -1.994333 0 0 0 1 -0.225536 0.838453 -0.496115 -2.099227 -0.031134 0.502773 0.863858 -1.392009 0.973737 0.210277 -0.087289 -1.994334 0 0 0 1 0.257518 0.957376 -0.130826 -2.893460 0.012854 0.131986 0.991168 -0.780703 0.966188 -0.256925 0.021683 -3.602867 0 0 0 1 0.225086 0.836690 -0.499286 -2.485926 0.011561 0.510106 0.860034 -1.506640 0.974270 -0.199354 0.105144 -3.795710 0 0 0 1 0.225086 0.836690 -0.499286 -3.424798 0.011561 0.510106 0.860034 -1.506639 0.974270 -0.199354 0.105144 -3.795709 0 0 0 1 -0 -0.972471 -0.233025 -1.168550 -0 -0.233025 0.972471 -0.564073 -1 0.000001 -0 2.885144 0 0 0 1 0 0.959552 -0.281533 -2.961374 -0.010661 0.281517 0.959497 -0.880024 0.999943 0.003001 0.010230 -2.894693 0 0 0 1 1 -0.000001 0.000001 -2.885145 0.000001 1 -0.000005 -3.098022 -0.000001 0.000005 1 -0.000021 0 0 0 1";
            Assert.IsTrue(controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArray));
            Assert.AreEqual(1024, controllerBindPose.Float_Array.Count);
            Assert.IsTrue(controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArray)); ;
            
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_candycane_a_MaterialFileNotAvailable()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\candycane_a.chr", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            colladaData.GenerateDaeObject();

            // Controller check
            var controller = daeObject.Library_Controllers.Controller[0];
            Assert.AreEqual("Controller", controller.ID);
            var skin = controller.Skin;
            Assert.AreEqual("#candycane_a-mesh", skin.source);
            Assert.AreEqual("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1", skin.Bind_Shape_Matrix.Value_As_String);
            Assert.AreEqual(3, skin.Source.Length);
            var controllerJoints = skin.Source.Where(a => a.ID == "Controller-joints").First();
            var controllerBindPose = skin.Source.Where(a => a.ID == "Controller-bind_poses").First();
            var controllerWeights = skin.Source.Where(a => a.ID == "Controller-weights").First();
            var joints = skin.Joints;
            Assert.AreEqual(8, controllerJoints.Name_Array.Count);
            Assert.AreEqual("Controller-joints-array", controllerJoints.Name_Array.ID);

            var bindPoseArray = "0 0 -1 0.023305 1 0 0 0 0 -1 0 0 0 0 0 1 -0.000089 0 -1 -0.000092 1 0.000008 -0.000089 0 0.000008 -1 0 0 0 0 0 1 -0.000091 0 -1 -0.026455 1 0.000008 -0.000091 0 0.000008";
            var bindPoseArrayNegZeros = "-0 -0 -1 0.023305 1 -0 -0 -0 -0 -1 0 -0 0 0 0 1 -0.000089 -0 -1 -0.000092 1 0.000008 -0.000089 -0 0.000008 -1 0 -0 0 0 0 1 -0.000091 -0 -1 -0.026455 1 0.000008";
            Assert.AreEqual(128, controllerBindPose.Float_Array.Count);
            Assert.IsTrue(controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArray) || controllerBindPose.Float_Array.Value_As_String.StartsWith(bindPoseArrayNegZeros));
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(2, actualMaterialsCount);

            // VisualScene
            var scene = daeObject.Library_Visual_Scene.Visual_Scene[0];
            Assert.AreEqual(2, scene.Node.Length);
            var armature = scene.Node[0];
            var instance = scene.Node[1];
            Assert.AreEqual("Armature", armature.ID);
            Assert.AreEqual("Bip01", armature.Name);
            Assert.AreEqual("-0 1 -0 0 -0 -0 -1 -0 -1 -0 0 0.023305 0 0 0 1", armature.Matrix[0].Value_As_String);
            Assert.AreEqual("Armature_hang_seg1", armature.node[0].ID);
            Assert.AreEqual("hang_seg1", armature.node[0].Name);
            Assert.AreEqual("1 0.000089 -0 0.023396 -0.000089 1 0.000009 0 0 -0.000009 1 -0 0 0 0 1", armature.node[0].Matrix[0].Value_As_String);
            Assert.AreEqual("Armature_hang_seg2", armature.node[0].node[0].ID);
            Assert.AreEqual("hang_seg2", armature.node[0].node[0].Name);
            Assert.AreEqual("1 0.000002 -0 0.026363 -0.000002 1 -0 -0 0 0 1 -0 0 0 0 1", armature.node[0].node[0].Matrix[0].Value_As_String);

            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_uac5_bh1_cga()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\hbr_right_torso_uac5_bh1.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            COLLADA colladaData = new COLLADA(testUtils.argsHandler, cryData);
            colladaData.GenerateDaeObject();
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            Assert.AreEqual(21, actualMaterialsCount);
            testUtils.ValidateColladaXml(colladaData);
        }

        [TestMethod]
        public void MWO_hbr_right_torso_cga()
        {
            var args = new string[] { $@"{userHome}\OneDrive\ResourceFiles\hbr_right_torso.cga", "-dds", "-dae", "-objectdir", @"..\..\ResourceFiles\" };
            int result = testUtils.argsHandler.ProcessArgs(args);
            Assert.AreEqual(0, result);
            CryEngine cryData = new CryEngine(args[0], testUtils.argsHandler.DataDir.FullName);
            cryData.ProcessCryengineFiles();

            var colladaData = new COLLADA(testUtils.argsHandler, cryData);
            var daeObject = colladaData.DaeObject;
            colladaData.GenerateDaeObject();

            Assert.AreEqual("Scene", daeObject.Scene.Visual_Scene.Name);
            Assert.AreEqual("#Scene", daeObject.Scene.Visual_Scene.URL);
            // Visual Scene Check
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene.Length);
            Assert.AreEqual("Scene", daeObject.Library_Visual_Scene.Visual_Scene[0].ID);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node.Length);
            // Node check
            var node = daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0];
            Assert.AreEqual("hbr_right_torso", node.ID);
            Assert.AreEqual("hbr_right_torso", node.Name);
            Assert.AreEqual(1, node.Instance_Geometry.Length);
            Assert.AreEqual(2, node.node.Length);
            Assert.AreEqual(1, node.Matrix.Length);
            Assert.AreEqual(1, node.Instance_Geometry.Length);
            Assert.AreEqual("hbr_right_torso_case", node.node[0].ID);
            Assert.AreEqual("hbr_right_torso_case", node.node[0].Name);
            Assert.AreEqual("hbr_right_torso_fx", node.node[1].Name);
            Assert.AreEqual(Grendgine_Collada_Node_Type.NODE, node.node[0].Type);
            const string caseMatrix = "-1.000000 -0.000005 0.000008 1.830486 0.000001 -0.866025 -0.500000 -2.444341 0.000009 -0.500000 0.866025 -1.542505 0.000000 0.000000 0.000000 1.000000";
            const string fxMatrix = "1.000000 0.000000 0.000009 1.950168 -0.000000 1.000000 -0.000000 0.630385 -0.000009 0.000000 1.000000 -0.312732 0.000000 0.000000 0.000000 1.000000";
            Assert.AreEqual(caseMatrix, node.node[0].Matrix[0].Value_As_String);
            Assert.AreEqual(fxMatrix, node.node[1].Matrix[0].Value_As_String);
            // Node Matrix check
            const string matrix = "1.000000 0.000000 0.000000 0.000000 0.000000 1.000000 0.000000 0.000000 0.000000 0.000000 1.000000 0.000000 0.000000 0.000000 0.000000 1.000000";
            Assert.AreEqual(matrix, node.Matrix[0].Value_As_String);
            Assert.AreEqual("transform", node.Matrix[0].sID);
            // Instance Geometry check
            Assert.AreEqual("hbr_right_torso", node.Instance_Geometry[0].Name);
            Assert.AreEqual("#hbr_right_torso-mesh", node.Instance_Geometry[0].URL);
            Assert.AreEqual(1, node.Instance_Geometry[0].Bind_Material.Length);
            Assert.AreEqual(1, node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material.Length);
            Assert.AreEqual("hellbringer_body-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Symbol);
            Assert.AreEqual("#hellbringer_body-material", node.Instance_Geometry[0].Bind_Material[0].Technique_Common.Instance_Material[0].Target);
            // library_materials Check
            int actualMaterialsCount = colladaData.DaeObject.Library_Materials.Material.Count();
            var materials = colladaData.DaeObject.Library_Materials;
            Assert.AreEqual(5, actualMaterialsCount);
            Assert.AreEqual("hellbringer_body-material", materials.Material[0].ID);
            Assert.AreEqual("decals-material", materials.Material[1].ID);
            Assert.AreEqual("hellbringer_variant-material", materials.Material[2].ID);
            Assert.AreEqual("hellbringer_window-material", materials.Material[3].ID);
            Assert.AreEqual("Material #0-material", materials.Material[4].ID);
            Assert.AreEqual("#hellbringer_body-effect", materials.Material[0].Instance_Effect.URL);
            Assert.AreEqual("#decals-effect", materials.Material[1].Instance_Effect.URL);
            Assert.AreEqual("#hellbringer_variant-effect", materials.Material[2].Instance_Effect.URL);
            Assert.AreEqual("#hellbringer_window-effect", materials.Material[3].Instance_Effect.URL);
            Assert.AreEqual("#Material #0-effect", materials.Material[4].Instance_Effect.URL);

            // library_geometries check
            Assert.AreEqual(1, colladaData.DaeObject.Library_Geometries.Geometry.Length);
            var geometry = colladaData.DaeObject.Library_Geometries.Geometry[0];
            Assert.AreEqual("hbr_right_torso-mesh", geometry.ID);
            Assert.AreEqual("hbr_right_torso", geometry.Name);
            Assert.AreEqual(4, geometry.Mesh.Source.Length);
            Assert.AreEqual("hbr_right_torso-vertices", geometry.Mesh.Vertices.ID);
            Assert.AreEqual(1, geometry.Mesh.Triangles.Length);
            Assert.AreEqual(1908, geometry.Mesh.Triangles[0].Count);
            var source = geometry.Mesh.Source;
            var vertices = geometry.Mesh.Vertices;
            var triangles = geometry.Mesh.Triangles;
            // Triangles check
            Assert.AreEqual("hellbringer_body-material", triangles[0].Material);
            Assert.AreEqual("#hbr_right_torso-mesh-pos", vertices.Input[0].source);
            Assert.IsTrue(triangles[0].P.Value_As_String.StartsWith("0 0 0 1 1 1 2 2 2 3 3 3 4 4 4 5 5 5 5 5 5 6 6 6 3 3 3 7 7 7 8 8 8 9 9 9 9 9 9"));
            // Source check
            Assert.AreEqual("hbr_right_torso-mesh-pos", source[0].ID);
            Assert.AreEqual("hbr_right_torso-pos", source[0].Name);
            Assert.AreEqual("hbr_right_torso-mesh-pos-array", source[0].Float_Array.ID);
            Assert.AreEqual(7035, source[0].Float_Array.Count);
            var floatArray = source[0].Float_Array.Value_As_String;
            Assert.IsTrue(floatArray.StartsWith("2.525999 -1.729837 -1.258107 2.526004 -1.863573 -1.080200 2.525999 -1.993050 -1.255200 2.740049 -0.917271 0.684382 2.740053 -0.917271 0.840976 2.793932"));
            Assert.IsTrue(floatArray.EndsWith("-3.263152 2.340879 -1.480840 -3.231119 2.352005 -1.494859 -3.268093 2.329598 -1.478497 -3.234514 2.335588 -1.491449 -3.273033 2.320036 -1.471824 -3.237391"));
            Assert.AreEqual((uint)2345, source[0].Technique_Common.Accessor.Count);
            Assert.AreEqual((uint)0, source[0].Technique_Common.Accessor.Offset);
            Assert.AreEqual(3, source[0].Technique_Common.Accessor.Param.Length);
            Assert.AreEqual("X", source[0].Technique_Common.Accessor.Param[0].Name);
            Assert.AreEqual("float", source[0].Technique_Common.Accessor.Param[0].Type);


            Assert.AreEqual("hbr_right_torso", daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].ID);
            Assert.AreEqual(1, daeObject.Library_Visual_Scene.Visual_Scene[0].Node[0].Instance_Geometry.Length);
            testUtils.ValidateColladaXml(colladaData);
        }
    }
}
