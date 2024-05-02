using AeonHacs.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static AeonHacs.Components.CegsPreferences;
using static AeonHacs.Utilities.Utility;

namespace AeonHacs.Components
{
    public partial class CegsMines : Cegs
    {
        #region HacsComponent

        [HacsPreConnect]
        protected virtual void PreConnect()
        {
            #region Logs

            SampleLog = Find<HacsLog>("SampleLog");

            VM1PressureLog = Find<DataLog>("VM1PressureLog");
            VM1PressureLog.Changed = (col) => col.Resolution > 0 && col.Source is Manometer m ?
                (col.PriorValue is double p ?
                    Manometer.SignificantChange(p, m.Pressure) :
                    true) :
                false;

            VM2PressureLog = Find<DataLog>("VM2PressureLog");
            VM2PressureLog.Changed = (col) => col.Resolution > 0 && col.Source is Manometer m ?
                (col.PriorValue is double p ?
                    Manometer.SignificantChange(p, m.Pressure) :
                    true) :
                false;

            AmbientLog = Find<DataLog>("AmbientLog");
            // These components are needed to allow the inclusion of
            // non-INamedValue properties of theirs in logged data.
            HeaterController1 = Find<HC6Controller>("HeaterController1");
            HeaterController2 = Find<HC6Controller>("HeaterController2");
            AmbientLog.AddNewValue("HC1.CJ", -1, "0.0",
                () => HeaterController1.ColdJunctionTemperature);
            AmbientLog.AddNewValue("HC2.CJ", -1, "0.0",
                () => HeaterController2.ColdJunctionTemperature);


            GRSTLog = Find<DataLog>("GRSampleTemperatureLog");
            // These components are needed to allow the inclusion of
            // non-INamedValue properties of theirs in logged data.
            GR1 = Find<GraphiteReactor>("GR1");
            GR2 = Find<GraphiteReactor>("GR2");
            GR3 = Find<GraphiteReactor>("GR3");
            GR4 = Find<GraphiteReactor>("GR4");
            GR5 = Find<GraphiteReactor>("GR5");
            GR6 = Find<GraphiteReactor>("GR6");
            GRSTLog.AddNewValue("GR1.SampleTemperature", 1, "0.0",
                () => GR1.SampleTemperature);
            GRSTLog.AddNewValue("GR2.SampleTemperature", 1, "0.0",
                () => GR2.SampleTemperature);
            GRSTLog.AddNewValue("GR3.SampleTemperature", 1, "0.0",
                () => GR3.SampleTemperature);
            GRSTLog.AddNewValue("GR4.SampleTemperature", 1, "0.0",
                () => GR4.SampleTemperature);
            GRSTLog.AddNewValue("GR5.SampleTemperature", 1, "0.0",
                () => GR5.SampleTemperature);
            GRSTLog.AddNewValue("GR6.SampleTemperature", 1, "0.0",
                () => GR6.SampleTemperature);

            #endregion Logs
        }

        [HacsConnect]
        protected override void Connect()
        {
            base.Connect();

            #region a Cegs needs these
            // The base Cegs really can't do "carbon extraction and graphitization"
            // unless these objects are defined.

            Power = Find<Power>("Power");
            Ambient = Find<Chamber>("Ambient");

            IM = Find<Section>("IM");
            VTT = Find<Section>("VTT");
            MC = Find<Section>("MC");
            Split = Find<Section>("Split");
            GM = Find<Section>("GM");

            VTT_MC = Find<Section>("VTT_MC");
            MC_Split = Find<Section>("MC_Split");

            ugCinMC = Find<Meter>("ugCinMC");

            InletPorts = CachedList<IInletPort>();
            GraphiteReactors = CachedList<IGraphiteReactor>();
            d13CPorts = CachedList<Id13CPort>();

            #endregion a Cegs needs these

            #region Cegs options
            // The Cegs doesn't require these, but provides
            // added functionality if they are present.

            #endregion Cegs options

            CT = Find<Section>("CT");
            IM_CT = Find<Section>("IM_CT");
            CT_VTT = Find<Section>("CT_VTT");
            MC_GM = Find<Section>("MC_GM");

            VS1All = Find<Section>("VS1All");
            VS2All = Find<Section>("VS2All");
            CA1 = Find<SableCA10>("CA1");
            TF1 = Find<SerialTubeFurnace>("TF1");

            VTT.Clean = () => Clean(VTT);


        }
        #endregion HacsComponent

        #region System configuration
        #region Component lists
        [JsonProperty] public Dictionary<string, TubeFurnace> TubeFurnaces { get; set; }
        [JsonProperty] public Dictionary<string, SableCA10> Analyzers { get; set; }
        [JsonProperty] public Dictionary<string, LnScale> LnScales { get; set; }
        #endregion Component lists

        #region HacsComponents
        public DataLog GRSTLog { get; set; }
        public GraphiteReactor GR1;
        public GraphiteReactor GR2;
        public GraphiteReactor GR3;
        public GraphiteReactor GR4;
        public GraphiteReactor GR5;
        public GraphiteReactor GR6;

        public HC6Controller HeaterController1;
        public HC6Controller HeaterController2;
        public HC6Controller HeaterController3;
        public HC6Controller HeaterController4;
        public virtual double umolCinMC => ugCinMC.Value / gramsCarbonPerMole;
        public virtual ISection IM_CT { get; set; }
        public virtual ISection CT_VTT { get; set; }
        public virtual ISection MC_GM { get; set; }
        public virtual ISection VS1All { get; set; }
        public virtual ISection VS2All { get; set; }

        public SableCA10 CA1 { get; set; }
        public SerialTubeFurnace TF1 { get; set; }

        #endregion HacsComponents
        #endregion System configuration

        #region Periodic system activities & maintenance

        protected override void ZeroPressureGauges()
        {
            base.ZeroPressureGauges();

            // do not auto-zero pressure gauges while a process is running
            if (Busy) return;

            bool OkToZeroManometer(ISection section) =>
                    section is Section s &&
                    s.VacuumSystem.TimeAtBaseline.TotalSeconds > 20 &&
                    (s.PathToVacuum?.IsOpened() ?? false);

            if (OkToZeroManometer(MC))
                ZeroIfNeeded(MC?.Manometer, 15);

            if (OkToZeroManometer(CT))
                ZeroIfNeeded(CT?.Manometer, 5);

            if (OkToZeroManometer(IM))
                ZeroIfNeeded(IM?.Manometer, 10);

            if (OkToZeroManometer(GM))
            {
                ZeroIfNeeded(GM?.Manometer, 10);
                foreach (var gr in GraphiteReactors)
                    if (Manifold(gr).PathToVacuum.IsOpened() && gr.IsOpened)
                        ZeroIfNeeded(gr.Manometer, 5);
            }
        }

        #endregion Periodic system activities & maintenance

        #region Process Management

        protected override void BuildProcessDictionary()
        {
            Separators.Clear();

            ProcessDictionary["Run samples"] = RunSamples;
            Separators.Add(ProcessDictionary.Count);

            ProcessDictionary["Prepare GRs for new iron and desiccant"] = PrepareGRsForService;
            ProcessDictionary["Precondition GR iron"] = PreconditionGRs;
            ProcessDictionary["Replace iron in sulfur traps"] = ChangeSulfurFe;
            Separators.Add(ProcessDictionary.Count);

            ProcessDictionary["Prepare carbonate sample for acid"] = PrepareCarbonateSample;
            ProcessDictionary["Load acidified carbonate sample"] = LoadCarbonateSample;
            Separators.Add(ProcessDictionary.Count);

            ProcessDictionary["Admit sealed CO2 to InletPort"] = AdmitSealedCO2IP;
            ProcessDictionary["Collect CO2 from InletPort"] = Collect;
            ProcessDictionary["Extract"] = Extract;
            ProcessDictionary["Measure"] = Measure;
            ProcessDictionary["Discard excess CO2 by splits"] = DiscardSplit;
            ProcessDictionary["Remove sulfur"] = RemoveSulfur;
            ProcessDictionary["Dilute small sample"] = Dilute;
            ProcessDictionary["Graphitize aliquots"] = GraphitizeAliquots;
            ProcessDictionary["Open and evacuate line"] = OpenLine;
            Separators.Add(ProcessDictionary.Count);

            ProcessDictionary["Collect, etc."] = CollectEtc;
            ProcessDictionary["Extract, etc."] = ExtractEtc;
            ProcessDictionary["Measure, etc."] = MeasureEtc;
            ProcessDictionary["Graphitize, etc."] = GraphitizeEtc;
            Separators.Add(ProcessDictionary.Count);

            ProcessDictionary["Evacuate Inlet Port"] = EvacuateIP;
            ProcessDictionary["Flush Inlet Port"] = FlushIP;
            ProcessDictionary["Admit O2 into Inlet Port"] = AdmitIPO2;
            ProcessDictionary["Heat Quartz and Open Line"] = HeatQuartzOpenLine;
            ProcessDictionary["Turn off IP furnace"] = TurnOffIPFurnace;
            ProcessDictionary["Discard IP gases"] = DiscardIPGases;
            ProcessDictionary["Close IP"] = CloseIP;
            ProcessDictionary["Bleed IP gas through frozen CT"] = FrozenBleed;
            ProcessDictionary["Bleed IP gas through CT (no temperature control)"] = Bleed;
            ProcessDictionary["Evacuate and Freeze VTT"] = FreezeVtt;
            ProcessDictionary["Clean VTT"] = CleanVtt;
            ProcessDictionary["Admit Dead CO2 into MC"] = AdmitDeadCO2;
            ProcessDictionary["Purify CO2 in MC"] = CleanupCO2InMC;
            ProcessDictionary["Discard MC gases"] = DiscardMCGases;
            ProcessDictionary["Divide sample into aliquots"] = DivideAliquots;
            ProcessDictionary["Wait for operator"] = WaitForOperator;
            Separators.Add(ProcessDictionary.Count);

            ProcessDictionary["Prepare loaded inlet ports for collection"] = PrepareIPsForCollection;
            ProcessDictionary["Transfer CO2 from MC to VTT"] = TransferCO2FromMCToVTT;
            // TODO implement this
            //			ProcessDictionary["Transfer CO2 from MC to CT"] = TransferCO2FromMCToCT;
            //            ProcessDictionary["Transfer CO2 from MC to IP"] = TransferCO2FromMCToIP;
            ProcessDictionary["Transfer CO2 from CT to VTT"] = TransferCO2FromCTToVTT;
            ProcessDictionary["Transfer CO2 from MC to GR"] = TransferCO2FromMCToGR;
            ProcessDictionary["Transfer CO2 from prior GR to MC"] = TransferCO2FromGRToMC;
            ProcessDictionary["Exercise all Opened valves"] = ExerciseAllValves;
            ProcessDictionary["Close all Opened valves"] = CloseAllValves;
            ProcessDictionary["Exercise all LN Manifold valves"] = ExerciseLNValves;
            ProcessDictionary["Calibrate all multi-turn valves"] = CalibrateRS232Valves;
            ProcessDictionary["Measure MC volume (KV in MCP1)"] = MeasureVolumeMC;
            ProcessDictionary["Measure valve volumes (plug in MCP1)"] = MeasureValveVolumes;
            ProcessDictionary["Measure remaining chamber volumes"] = MeasureRemainingVolumes;
            ProcessDictionary["Check GR H2 density ratios"] = CalibrateGRH2;
            ProcessDictionary["Measure Extraction efficiency"] = MeasureExtractEfficiency;
            ProcessDictionary["Measure IP collection efficiency"] = MeasureIpCollectionEfficiency;
            ProcessDictionary["Test"] = Test;

            base.BuildProcessDictionary();
        }

        protected virtual void CleanVtt() => VTT.Clean();

        protected virtual void CloseGasSupplies()
        {
            ProcessSubStep.Start("Close gas supplies");
            bool isCegsGasSupply(GasSupply gs) =>
                VacuumSystems.ContainsValue(gs.Destination.VacuumSystem);

            foreach (GasSupply g in GasSupplies.Values)
            {
                // Look only in CEGS vacuum systems; ignore other process managers
                if (isCegsGasSupply(g))
                    g.ShutOff();
            }

            // close gas flow valves after all shutoff valves are closed
            foreach (GasSupply g in GasSupplies.Values)
            {
                if (isCegsGasSupply(g))
                    g.FlowValve?.CloseWait();
            }

            ProcessSubStep.End();

        }

        protected override void OpenLine() =>
            FastOpenLine();

        // TODO: this is too slow and lossy to be practical
        // Instead, transfer the CO2 from the MC to a valved vessel
        // on a GR port. Then move the vessel to the IP, Collect(),
        // Extract(), and Measure().
        protected override void TransferCO2FromMCToIP()
        {
            if (!IpIm(out ISection im)) return;
            var mc = MC.Coldfinger;

            ProcessStep.Start($"Thaw the {MC.Name}");
            MC.Isolate();
            if (!mc.Thawed)
                MC.Thaw();
            ProcessStep.End();

            ProcessStep.Start($"Evacuate and join {InletPort.Name} and {Split.Name}");
            im.ClosePortsExcept(InletPort);
            InletPort.Open();

            im.OpenAndEvacuate();
            Split.OpenAndEvacuate();
            im.PathToVacuum.Open();
            WaitForStablePressure(im.VacuumSystem, CleanPressure);
            InletPort.Close();
            ProcessStep.End();

            ProcessStep.Start($"Transfer CO2 from MC to {InletPort.Name}");
            Alert("Operator Needed", "Put LN on inlet port.");
            Notice.Send("Operator needed", "Almost ready for LN on inlet port.\r\n" +
                "Press Ok to continue, then raise LN onto inlet port tube");

            ProcessSubStep.Start($"Wait for {MC.Name} to warm up a bit.");
            WaitFor(() => mc.Temperature > CO2TransferStartTemperature);
            ProcessSubStep.End();

            im.VacuumSystem.Isolate();
            MC_Split.Open();
            WaitSeconds(3);
            InletPort.Open();

            // Molecular flow is reached almost immediately, at around 5 Torr. Thereafter,
            // the remainder of this transfer takes an extremely long time. Of a 900-ugC
            // sample, probably ~8 ugC will remain in the MC after 45 minutes, and a much
            // larger fraction will be left in the VM and IM.
            WaitMinutes(45);

            Alert("Operator Needed", "Raise inlet port LN.");
            Notice.Send("Operator needed", $"Raise {InletPort.Name} LN one inch.\r\n" +
                "Press Ok to continue.");

            WaitSeconds(30);

            InletPort.Close();
            ProcessStep.End();
        }


        /// <summary>
        /// Event handler for MC temperature and pressure changes
        /// </summary>
        protected override void UpdateSampleMeasurement(object sender = null, PropertyChangedEventArgs e = null)
        {
            var ugC = ugCinMC.Value;
            base.UpdateSampleMeasurement(sender, e);
            if (ugCinMC.Value != ugC)
                NotifyPropertyChanged(nameof(umolCinMC));
        }

        #endregion Process Management

        #region Test functions
        void ValvePositionDriftTest()
        {
            var v = FirstOrDefault<RS232Valve>();
            var pos = v.ClosedValue / 2;
            var op = new ActuatorOperation()
            {
                Name = "test",
                Value = pos,
                Incremental = false
            };
            v.ActuatorOperations.Add(op);

            v.DoWait(op);

            //op.Incremental = true;
            var rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                op.Value = pos + rand.Next(-15, 16);
                v.DoWait(op);
            }
            op.Value = pos;
            op.Incremental = false;
            v.DoWait(op);

            v.ActuatorOperations.Remove(op);
        }

        void TestPort(IPort p)
        {
            for (int i = 0; i < 5; ++i)
            {
                p.Open();
                p.Close();
            }
            p.Open();
            WaitMinutes(5);
            p.Close();
        }

        // two minutes of moving the valve at a moderate pace
        void TestValve(IValve v)
        {
            SampleLog.Record($"Operating {v.Name} for 2 minutes");
            for (int i = 0; i < 24; ++i)
            {
                v.CloseWait();
                WaitSeconds(2);
                v.OpenWait();
                WaitSeconds(2);
            }
        }

        void TestUpstream(IValve v)
        {
            SampleLog.Record($"Checking {v.Name}'s 10-minute bump");
            v.OpenWait();
            WaitMinutes(5);     // empty the upstream side (assumes the downstream side is under vacuum)
            v.CloseWait();
            WaitMinutes(10);    // let the upstream pressure rise for 10 minutes
            v.OpenWait();       // how big is the pressure bump?
        }


        protected virtual void ExercisePorts(ISection s)
        {
            s.Isolate();
            s.Open();
            s.OpenPorts();
            WaitSeconds(5);
            s.ClosePorts();
            s.Evacuate(OkPressure);
        }

        protected virtual void FastOpenLine()
        {
            CloseGasSupplies();
            if (!VS1All.IsOpened || !VS1All.PathToVacuum.IsOpened())
                VS1All.OpenAndEvacuate();
            if (!VS2All.IsOpened || !VS2All.PathToVacuum.IsOpened())
                VS2All.OpenAndEvacuate();
            WaitFor(() => VS1All.VacuumSystem.Pressure <= OkPressure && VS2All.VacuumSystem.Pressure <= OkPressure);
            Section.Connections(VS1All, VS2All).Open();
        }

        protected void CalibrateManualHeaters()
        {
            var tc = Find<IThermocouple>("tCal");
            CalibrateManualHeater(Find<IHeater>("hIP1CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP2CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP3CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP4CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP5CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP6CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP7CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP8CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP9CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP10CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP11CCQ"), tc);
            CalibrateManualHeater(Find<IHeater>("hIP12CCQ"), tc);
        }

        protected virtual void TestAdmit(string gasSupply, double pressure)
        {
            var gs = Find<GasSupply>(gasSupply);
            gs?.Destination?.OpenAndEvacuate();
            gs?.Destination?.ClosePorts();
            gs?.Admit(pressure);
            WaitSeconds(10);
            SampleLog.Record($"Admit test: {gasSupply}, target: {pressure:0.###}, stabilized: {gs.Meter.Value:0.###} in {ProcessStep.Elapsed:m':'ss}");
            gs?.Destination?.OpenAndEvacuate();
        }

        protected virtual void TestPressurize(string gasSupply, double pressure)
        {
            var gs = Find<GasSupply>(gasSupply);
            gs?.Destination?.OpenAndEvacuate(OkPressure);
            gs?.Destination?.ClosePorts();
            gs?.Pressurize(pressure);
            WaitSeconds(60);
            SampleLog.Record($"Pressurize test: {gasSupply}, target: {pressure:0.###}, stabilized: {gs.Meter.Value:0.###} in {ProcessStep.Elapsed:m':'ss}");
            gs?.Destination?.OpenAndEvacuate();
        }

        protected virtual void TestGasSupplies()

        {
            TestAdmit("O2.IM", IMO2Pressure);
            TestPressurize("CO2.MC", 75);
            TestPressurize("CO2.MC", 0.95 * MicrogramsCarbon(MC.Manometer.MaxValue, MC.MilliLiters, MC.Temperature));
            TestAdmit("He.IM", 800);
            TestPressurize("He.MC", 80);
            TestPressurize("He.AM", PressureOverAtm);
            TestAdmit("He.GM", 760);
            TestPressurize("H2.GM", 100);
            TestPressurize("H2.GM", 900);
        }

        public void VttWarmStepTest()
        {
            if (Find<VTColdfinger>("VTC") is not VTColdfinger vtc)
                return;
            if (vtc.Heater is not IHeater h)
                return;

            // Replace VTC heater with dummy so we can manually control the real heater.
            vtc.Heater = new HC6Heater();

            double initialCO = 0.1;
            double stepCO = 0.3;

            h.Setpoint = 50;
            h.Manual(initialCO);
            h.TurnOn();

            WaitMinutes(60); // Wait for temperature to stabalize.

            var log = new DataLog($"VttWarm Step Test.txt")
            {
                Columns = new()
                {
                    new DataLog.Column()
                    {
                        Name = "tVTC",
                        Resolution = -1.0,
                        Format = "0.00"
                    },
                    new DataLog.Column()
                    {
                        Name = vtc.TopThermometer.Name,
                        Resolution = -1.0,
                        Format = "0.00"
                    },
                    new DataLog.Column()
                    {
                        Name = vtc.WireThermometer.Name,
                        Resolution = -1.0,
                        Format = "0.00"
                    }
                },
                ChangeTimeoutMilliseconds = 3 * 1000,
                OnlyLogWhenChanged = false,
                InsertLatestSkippedEntry = false
            };

            HacsLog.List.Add(log);

            h.PowerLevel = stepCO;

            WaitMinutes(40);

            HacsLog.List.Remove(log);
            log.Close();
            log = null;

            h.TurnOff();
            h.Auto();
            vtc.Heater = h;
        }


        protected override void Test()
        {
            //Find<IVolumeCalibration>("CT1, CT2, CTF, IM").Calibrate();
            ////Find<IVolumeCalibration>("VM1").Calibrate();
            //return;

            var tc = Find<IThermocouple>("tCal");
            //CalibrateManualHeater(Find<IHeater>("hIP1CCQ"), tc);
            //CalibrateManualHeater(Find<IHeater>("hIP2CCQ"), tc);

            //CalibrateManualHeaters();
            //return;

            //var ips = new List<IInletPort>()
            //{
            //    Find<IInletPort>("IP2")
            //};
            //ips.ForEach(ip => ip.QuartzFurnace.TurnOn());
            //WaitMinutes(10);
            //PidStepTest(ips.Select(ip => ip.SampleFurnace).Cast<IHeater>().ToArray());
            //ips.ForEach(ip => ip.QuartzFurnace.TurnOff());
            //return;

            //var grs = new List<IHeater>()
            //{
            //    Find<IHeater>("hGR2"),
            //    Find<IHeater>("hGR4"),
            //    Find<IHeater>("hGR6"),
            //    //Find<IHeater>("hGR7"),
            //    //Find<IHeater>("hGR9"),
            //    //Find<IHeater>("hGR11")
            //}.ToArray();
            //PidStepTest(grs);
            //return;

            //VttWarmStepTest();
            //return;


            //TestPressurize("CO2.MC", 0.99 * MicrogramsCarbon(MC.Manometer.MaxValue, MC.MilliLiters, MC.Temperature));
            //TestGasSupplies();
            //return;


            //FastOpenLine();
            //for (int i = 0; i < 100; ++i)
            //{
            //    //ExercisePorts(IM);
            //    //ExercisePorts(GM);

            //    //MC.PathToVacuum?.Open();     // Opens GM, too
            //    //VTT.PathToVacuum?.Open();
            //    //IM.PathToVacuum?.Open();
            //    //IM_CT.Open();
            //    //VTT_MC.Open();

            //    var list = FindAll<CpwValve>(v => v.IsOpened && !(v is RS232Valve));
            //    list.ForEach(v => 
            //    {
            //        v.CloseWait();
            //        v.OpenWait();
            //    });
            //    WaitMinutes(30);
            //}

            //for (int i = 0; i < 5; ++i)
            //{
            //    TestValve(Find<IValve>("vIML_IMC"));
            //    TestValve(Find<IValve>("vIMR_IMC"));

            //    TestValve(Find<IValve>("vIMC_CT"));
            //    TestValve(Find<IValve>("vCT_VTT"));
            //    TestValve(Find<IValve>("vVTT_MC"));
            //    TestValve(Find<IValve>("vMC_MCP1"));
            //    TestValve(Find<IValve>("vMC_MCP2"));
            //    TestValve(Find<IValve>("vMC_Split"));

            //    TestValve(Find<IValve>("vGML_GMC"));
            //    TestValve(Find<IValve>("vGMR_GMC"));

            //    TestValve(Find<IValve>("vIMC_VM"));
            //    TestValve(Find<IValve>("vCT_VM"));
            //    TestValve(Find<IValve>("vGMC_VM"));

            //}
            //return;

            //TestPort(Find<IPort>("IP2"));
            //TestPort(Find<IPort>("IP3"));
            //TestPort(Find<IPort>("IP4"));
            //TestPort(Find<IPort>("IP5"));
            //TestPort(Find<IPort>("IP6"));

            //TestPort(Find<IPort>("GR7"));
            //TestPort(Find<IPort>("GR8"));
            //TestPort(Find<IPort>("GR9"));
            //TestPort(Find<IPort>("GR10"));
            //TestPort(Find<IPort>("GR11"));
            //TestPort(Find<IPort>("GR12"));

            //MC.Evacuate(OkPressure);
            //TestValve(Find<IValve>("v_MCP0"));
            //return;

            //ProcessStep.Start("Simulating Sample Run");
            //Wait(10000);
            //ProcessStep.End();

            //Admit("O2", IM, null, IMO2Pressure);

            //var gs = Find<GasSupply>("H2.GM");
            //gs.Pressurize(100);
            //gs.Pressurize(900);

            //var gs = Find<GasSupply>("He.GM");
            //gs.Admit(800);
            //WaitSeconds(10);

            //var gs = Find<GasSupply>("He.IM");
            //gs.Destination.Evacuate(OkPressure);

            //gs.Admit(800);
            //WaitSeconds(10);
            //gs.Destination.Evacuate(OkPressure);

            //gs = Find<GasSupply>("He.MC");
            //gs.Destination.Evacuate(OkPressure);

            //gs.Pressurize(95);
            //WaitSeconds(10);
            //gs.Destination.Evacuate(OkPressure);

            //gs = Find<GasSupply>("CO2.MC");
            //gs.Destination.Evacuate(OkPressure);

            //gs.Pressurize(75);
            //WaitSeconds(10);
            //gs.Destination.Evacuate(OkPressure);

            //gs.Pressurize(1000);
            //WaitSeconds(10);
            //gs.Destination.Evacuate(OkPressure);

            //InletPort = Find<InletPort>("IP1");
            //AdmitIPO2();
            //Collect();

            //var grs = new List<IGraphiteReactor>();
            //grs.AddRange(GraphiteReactors.Where(gr => gr.Prepared));
            //CalibrateGRH2(grs);

            //var gr1 = Find<GraphiteReactor>("GR1");
            //var gr2 = Find<GraphiteReactor>("GR2");
            //GrGmH2(gr1, out ISection gm, out IGasSupply gs);
            //gr1.Open();
            //gr2.Open();
            //gm.Evacuate(OkPressure);
            //gr1.Close();
            //gr2.Close();

            //gs.Pressurize(IronPreconditionH2Pressure);

            //var p1 = gm.Manometer.WaitForAverage(60);
            //gr1.Open();
            //WaitSeconds(10);
            //gr1.Close();
            //WaitSeconds(10);
            //var p2 = gm.Manometer.WaitForAverage(60);
            //SampleLog.Record($"dpGM for GR1: {p1:0.00} => {p2:0.00}");

            //p1 = gm.Manometer.WaitForAverage(60);
            //gr2.Open();
            //WaitSeconds(10);
            //gr2.Close();
            //WaitSeconds(10);
            //p2 = gm.Manometer.WaitForAverage(60);
            //SampleLog.Record($"dpGM for GR2: {p1:0.00} => {p2:0.00}");

            // Test CTFlowManager
            // Control flow valve to maintain constant downstream pressure until flow valve is fully opened.
            //SampleLog.Record($"Bleed pressure: {FirstTrapBleedPressure} Torr");
            //Bleed(FirstTrap, FirstTrapBleedPressure);

            // Open flow bypass when conditions allow it without producing an excessive
            // downstream pressure spike. Then wait for the spike to be evacuated.
            //ProcessSubStep.Start("Wait for remaining pressure to bleed down");
            //WaitFor(() => IM.Pressure - FirstTrap.Pressure < FirstTrapFlowBypassPressure);
            //FirstTrap.Open();   // open bypass if available
            //WaitFor(() => FirstTrap.Pressure < FirstTrapEndPressure);
            //ProcessSubStep.End();


            //VolumeCalibrations["GR1, GR2"]?.Calibrate();
            //return;
        }

        #endregion Test functions

    }
}