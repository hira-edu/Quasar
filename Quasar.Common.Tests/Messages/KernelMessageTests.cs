using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using Quasar.Common.Enums;
using Quasar.Common.Messages;
using System.IO;

namespace Quasar.Common.Tests.Messages
{
    [TestClass]
    public class KernelMessageTests
    {
        [TestMethod, TestCategory("Messages")]
        public void DoKernelUnblock_RoundTrips()
        {
            var message = new DoKernelUnblock
            {
                ProcessName = "target",
                IncludeChildProcesses = true,
                RequireDriver = true,
                ForceResetAffinity = true,
                ExpectedDriverState = KernelDriverState.Running,
                DriverAction = KernelDriverAction.EnsureRunning,
                Force = true
            };

            var clone = RoundTrip(message);

            Assert.AreEqual(message.ProcessName, clone.ProcessName);
            Assert.AreEqual(message.IncludeChildProcesses, clone.IncludeChildProcesses);
            Assert.AreEqual(message.RequireDriver, clone.RequireDriver);
            Assert.AreEqual(message.ForceResetAffinity, clone.ForceResetAffinity);
            Assert.AreEqual(message.ExpectedDriverState, clone.ExpectedDriverState);
            Assert.AreEqual(message.DriverAction, clone.DriverAction);
            Assert.AreEqual(message.Force, clone.Force);
        }

        [TestMethod, TestCategory("Messages")]
        public void GetKernelDriverStatus_RoundTrips()
        {
            var message = new GetKernelDriverStatus
            {
                DriverAction = KernelDriverAction.Restart,
                ForceRefresh = true
            };

            var clone = RoundTrip(message);

            Assert.AreEqual(message.DriverAction, clone.DriverAction);
            Assert.AreEqual(message.ForceRefresh, clone.ForceRefresh);
        }

        [TestMethod, TestCategory("Messages")]
        public void KernelDriverStatusResponse_RoundTrips()
        {
            var message = new KernelDriverStatusResponse
            {
                State = KernelDriverState.Running,
                Version = "1.0.0",
                WatchdogActive = true,
                Message = "none"
            };

            var clone = RoundTrip(message);

            Assert.AreEqual(message.State, clone.State);
            Assert.AreEqual(message.Version, clone.Version);
            Assert.AreEqual(message.WatchdogActive, clone.WatchdogActive);
            Assert.AreEqual(message.Message, clone.Message);
        }

        [TestMethod, TestCategory("Messages")]
        public void GetDesktop_RoundTrips()
        {
            var message = new GetDesktop
            {
                CreateNew = true,
                Quality = 85,
                DisplayIndex = 2,
                IncludeCursor = false,
                ForceAffinityReset = true
            };

            var clone = RoundTrip(message);

            Assert.AreEqual(message.CreateNew, clone.CreateNew);
            Assert.AreEqual(message.Quality, clone.Quality);
            Assert.AreEqual(message.DisplayIndex, clone.DisplayIndex);
            Assert.AreEqual(message.IncludeCursor, clone.IncludeCursor);
            Assert.AreEqual(message.ForceAffinityReset, clone.ForceAffinityReset);
        }

        [TestMethod, TestCategory("Messages")]
        public void GetDesktopResponse_RoundTrips()
        {
            var message = new GetDesktopResponse
            {
                Image = new byte[] { 1, 2, 3 },
                Quality = 90,
                Monitor = 1,
                Resolution = new Quasar.Common.Video.Resolution { Height = 1080, Width = 1920 },
                DriverState = KernelDriverState.Running,
                FrameId = 42
            };

            var clone = RoundTrip(message);

            CollectionAssert.AreEqual(message.Image, clone.Image);
            Assert.AreEqual(message.Quality, clone.Quality);
            Assert.AreEqual(message.Monitor, clone.Monitor);
            Assert.AreEqual(message.Resolution.Height, clone.Resolution.Height);
            Assert.AreEqual(message.Resolution.Width, clone.Resolution.Width);
            Assert.AreEqual(message.DriverState, clone.DriverState);
            Assert.AreEqual(message.FrameId, clone.FrameId);
        }

        [TestMethod, TestCategory("Messages")]
        public void DoInputUnblock_RoundTrips()
        {
            var message = new DoInputUnblock
            {
                UnblockMouse = true,
                UnblockKeyboard = false,
                ForceBlockInputReset = true,
                ForceHookCleanup = true
            };

            var clone = RoundTrip(message);

            Assert.AreEqual(message.UnblockMouse, clone.UnblockMouse);
            Assert.AreEqual(message.UnblockKeyboard, clone.UnblockKeyboard);
            Assert.AreEqual(message.ForceBlockInputReset, clone.ForceBlockInputReset);
            Assert.AreEqual(message.ForceHookCleanup, clone.ForceHookCleanup);
        }

        [TestMethod, TestCategory("Messages")]
        public void InputUnblockResult_RoundTrips()
        {
            var message = new InputUnblockResult
            {
                ResultCode = InputUnblockResultCode.Success,
                MouseUnlocked = true,
                KeyboardUnlocked = true,
                DurationMilliseconds = 250,
                Message = "Cleared BlockInput."
            };

            var clone = RoundTrip(message);

            Assert.AreEqual(message.ResultCode, clone.ResultCode);
            Assert.AreEqual(message.MouseUnlocked, clone.MouseUnlocked);
            Assert.AreEqual(message.KeyboardUnlocked, clone.KeyboardUnlocked);
            Assert.AreEqual(message.DurationMilliseconds, clone.DurationMilliseconds);
            Assert.AreEqual(message.Message, clone.Message);
        }

        [TestMethod, TestCategory("Messages")]
        public void KernelUnblockResult_RoundTrips()
        {
            var message = new KernelUnblockResult
            {
                Result = KernelUnblockResultCode.Success,
                ProcessName = "demo",
                WindowsUpdated = 42,
                ProcessesInspected = 3,
                Message = "ok",
                ElapsedMilliseconds = 1500,
                DriverState = KernelDriverState.Running
            };

            var clone = RoundTrip(message);

            Assert.AreEqual(message.Result, clone.Result);
            Assert.AreEqual(message.ProcessName, clone.ProcessName);
            Assert.AreEqual(message.WindowsUpdated, clone.WindowsUpdated);
            Assert.AreEqual(message.ProcessesInspected, clone.ProcessesInspected);
            Assert.AreEqual(message.Message, clone.Message);
            Assert.AreEqual(message.ElapsedMilliseconds, clone.ElapsedMilliseconds);
            Assert.AreEqual(message.DriverState, clone.DriverState);
        }

        private static T RoundTrip<T>(T message)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, message);
                ms.Position = 0;
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}
