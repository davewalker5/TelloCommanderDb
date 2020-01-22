﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TelloCommander.Data.Collector;
using TelloCommander.Data.Entities;
using TelloCommander.Data.InMemory;
using TelloCommander.Interfaces;
using TelloCommander.Simulator;

namespace TelloCommander.Data.Tests
{
    [TestClass]
    public class TelemetryCollectorTest
    {
        private const string DroneName = "TELLO-1234";
        private readonly string SessionName = $"{DroneName} telemetry session";

        private IDroneStatusMonitor _monitor;
        private TelloCommanderDbContext _context;
        private TelemetryCollector _collector;

        [TestInitialize]
        public void TestInitialise()
        {
            _context = new TelloCommanderDbContextFactory().CreateDbContext(null);
            _monitor = new MockDroneStatusMonitor();
            _collector = new TelemetryCollector(_context, _monitor);
            _monitor.Listen(0);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _monitor.Stop();
        }

        [TestMethod]
        public void CollectionTest()
        {
            DateTime start = DateTime.Now;
            _collector.Start(DroneName, SessionName, 1000, null);
            DateTime end = DateTime.Now;
            Thread.Sleep(3000);
            _collector.Stop();

            Assert.AreEqual(1, _context.Drones.Count());
            Drone drone = _context.Drones.First();
            Assert.AreEqual(DroneName, drone.Name);

            Assert.AreEqual(1, _context.Sessions.Count());
            TelemetrySession session = _context.Sessions.First();
            Assert.AreEqual(drone.Id, session.DroneId);
            Assert.AreEqual(SessionName, session.Name);
            Assert.IsTrue(session.Start >= start);
            Assert.IsTrue(session.Start <= end);

            Assert.IsTrue(_context.Properties.Count() > 0);
            IEnumerable<string> propertyNames = _context.Properties.Select(p => p.Name).Distinct();
            Assert.IsTrue(propertyNames.Count() > 1);

            Assert.IsTrue(_context.DataPoints.Count() > 0);
        }
    }
}