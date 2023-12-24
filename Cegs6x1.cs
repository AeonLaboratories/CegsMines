using AeonHacs.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using static AeonHacs.Components.CegsPreferences;
using static AeonHacs.Utilities.Utility;

namespace AeonHacs.Components
{
    public partial class Cegs6x1 : Cegs
    {
        #region HacsComponent

        [HacsPreConnect]
        protected virtual void PreConnect()
        {
            #region Logs

            SampleLog = Find<HacsLog>("SampleLog");

            VMPressureLog = Find<DataLog>("VMPressureLog");
            VMPressureLog.Changed = (col) => col.Resolution > 0 && col.Source is Manometer m ?
                (col.PriorValue is double p ?
                    Manometer.SignificantChange(p, m.Pressure) :
                    true) :
                false;

            AmbientLog = Find<DataLog>("AmbientLog");
            // These components are needed to allow the inclusion of
            // non-INamedValue properties of theirs in logged data.
            HeaterController1 = Find<HC6Controller>("HeaterController1");
            HeaterController2 = Find<HC6Controller>("HeaterController2");
            HeaterController3 = Find<HC6Controller>("HeaterController3");
            HeaterController4 = Find<HC6Controller>("HeaterController4");
            HeaterController5 = Find<HC6Controller>("HeaterController5");
            HeaterController6 = Find<HC6Controller>("HeaterController6");
            HeaterController7 = Find<HC6Controller>("HeaterController7");
            AmbientLog.AddNewValue("HC1.CJ", -1, "0.0",
                () => HeaterController1.ColdJunctionTemperature);
            AmbientLog.AddNewValue("HC2.CJ", -1, "0.0",
                () => HeaterController2.ColdJunctionTemperature);
            AmbientLog.AddNewValue("HC3.CJ", -1, "0.0",
                () => HeaterController3.ColdJunctionTemperature);
            AmbientLog.AddNewValue("HC4.CJ", -1, "0.0",
                () => HeaterController4.ColdJunctionTemperature);
            AmbientLog.AddNewValue("HC5.CJ", -1, "0.0",
                () => HeaterController5.ColdJunctionTemperature);
            AmbientLog.AddNewValue("HC6.CJ", -1, "0.0",
                () => HeaterController6.ColdJunctionTemperature);
            AmbientLog.AddNewValue("HC7.CJ", -1, "0.0",
                () => HeaterController7.ColdJunctionTemperature);


            GRSTLog = Find<DataLog>("GRSampleTemperatureLog");
            // These components are needed to allow the inclusion of
            // non-INamedValue properties of theirs in logged data.
            GR1 = Find<GraphiteReactor>("GR1");
            GR2 = Find<GraphiteReactor>("GR2");
            GR3 = Find<GraphiteReactor>("GR3");
            GR4 = Find<GraphiteReactor>("GR4");
            GR5 = Find<GraphiteReactor>("GR5");
            GR6 = Find<GraphiteReactor>("GR6");
            GR7 = Find<GraphiteReactor>("GR7");
            GR8 = Find<GraphiteReactor>("GR8");
            GR9 = Find<GraphiteReactor>("GR9");
            GR10 = Find<GraphiteReactor>("GR10");
            GR11 = Find<GraphiteReactor>("GR11");
            GR12 = Find<GraphiteReactor>("GR12");
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
            GRSTLog.AddNewValue("GR7.SampleTemperature", 1, "0.0",
                () => GR7.SampleTemperature);
            GRSTLog.AddNewValue("GR8.SampleTemperature", 1, "0.0",
                () => GR8.SampleTemperature);
            GRSTLog.AddNewValue("GR9.SampleTemperature", 1, "0.0",
                () => GR9.SampleTemperature);
            GRSTLog.AddNewValue("GR10.SampleTemperature", 1, "0.0",
                () => GR10.SampleTemperature);
            GRSTLog.AddNewValue("GR11.SampleTemperature", 1, "0.0",
                () => GR11.SampleTemperature);
            GRSTLog.AddNewValue("GR12.SampleTemperature", 1, "0.0",
                () => GR12.SampleTemperature);

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
            VacuumSystem = Find<VacuumSystem>("VacuumSystem");

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

            VTT.Clean = () => Clean(VTT);

        }
        #endregion HacsComponent

        #region System configuration
        #region HacsComponents
        public DataLog GRSTLog { get; set; }
        public GraphiteReactor GR1;
        public GraphiteReactor GR2;
        public GraphiteReactor GR3;
        public GraphiteReactor GR4;
        public GraphiteReactor GR5;
        public GraphiteReactor GR6;
        public GraphiteReactor GR7;
        public GraphiteReactor GR8;
        public GraphiteReactor GR9;
        public GraphiteReactor GR10;
        public GraphiteReactor GR11;
        public GraphiteReactor GR12;

        public HC6Controller HeaterController1;
        public HC6Controller HeaterController2;
        public HC6Controller HeaterController3;
        public HC6Controller HeaterController4;
        public HC6Controller HeaterController5;
        public HC6Controller HeaterController6;
        public HC6Controller HeaterController7;
        public virtual double umolCinMC => ugCinMC.Value / gramsCarbonPerMole;
        public virtual ISection IM_CT { get; set; }
        public virtual ISection CT_VTT { get; set; }
        public virtual ISection MC_GM { get; set; }

        #endregion HacsComponents
        #endregion System configuration

        #region Periodic system activities & maintenance

        protected override void ZeroPressureGauges()
        {
            base.ZeroPressureGauges();

            // do not auto-zero pressure gauges while a process is running
            if (Busy) return;

            // ensure baseline VM pressure & steady state
            if (VacuumSystem.TimeAtBaseline.TotalSeconds < 20)
                return;

            if (MC?.PathToVacuum?.IsOpened() ?? false)
                ZeroIfNeeded(MC?.Manometer, 15);

            if (CT?.PathToVacuum?.IsOpened() ?? false)
                ZeroIfNeeded(CT?.Manometer, 5);

            if (IM?.PathToVacuum?.IsOpened() ?? false)
                ZeroIfNeeded(IM?.Manometer, 10);

            if (GM?.PathToVacuum?.IsOpened() ?? false)
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
            ProcessDictionary["Measure MC volume (KV in MCP2)"] = MeasureVolumeMC;
            ProcessDictionary["Measure valve volumes (plug in MCP2)"] = MeasureValveVolumes;
            ProcessDictionary["Measure remaining chamber volumes"] = MeasureRemainingVolumes;
            ProcessDictionary["Check GR H2 density ratios"] = CalibrateGRH2;
            ProcessDictionary["Measure Extraction efficiency"] = MeasureExtractEfficiency;
            ProcessDictionary["Measure IP collection efficiency"] = MeasureIpCollectionEfficiency;
            ProcessDictionary["Test"] = Test;

            base.BuildProcessDictionary();
        }

        protected virtual void CleanVtt() => VTT.Clean();

        protected override void OpenLine()
        {
            ProcessStep.Start("Open line");

            ProcessSubStep.Start("Close gas supplies");
            foreach (GasSupply g in GasSupplies.Values)
            {
                if (g.Destination.VacuumSystem == VacuumSystem)
                    g.ShutOff();
            }

            // close gas flow valves after all shutoff valves are closed
            foreach (GasSupply g in GasSupplies.Values)
            {
                if (g.Destination.VacuumSystem == VacuumSystem)
                    g.FlowValve?.CloseWait();
            }

            ProcessSubStep.End();

            Evacuate(OkPressure);

            Clean(VTT);
            Clean(CT);

            var gmWasOpened = GM.IsOpened && PreparedGRsAreOpened();
            var mcWasOpened = MC_Split.IsOpened && MC.Ports.All(p => p.IsOpened);
            var ctWasOpened = CT_VTT.IsOpened;
            var imWasOpened = IM.IsOpened;

            if (gmWasOpened && mcWasOpened && ctWasOpened && imWasOpened && IM_CT.IsOpened && VTT_MC.IsOpened)
            {
                Evacuate();
                ProcessStep.End();
                return;
            }

            if (!mcWasOpened)
            {
                ProcessSubStep.Start($"Evacuate {MC_Split.Name}");
                VacuumSystem.IsolateManifold();
                MC_Split.OpenAndEvacuateAll(OkPressure);        // include MC aliquot ports
                ProcessSubStep.End();
            }

            if (!gmWasOpened)
            {
                ProcessSubStep.Start($"Evacuate {GM.Name} and prepared GRs");
                VacuumSystem.IsolateManifold();
                GM.Isolate();
                OpenPreparedGRs();
                GM.OpenAndEvacuate(OkPressure);
                ProcessSubStep.End();
            }
            else
            {
                GM.InternalValves.Open(); // ensure GM internal valves are open in case MC evacuation closed them.
            }

            if (!ctWasOpened)
            {
                ProcessSubStep.Start($"Evacuate {CT_VTT.Name}");
                VacuumSystem.IsolateManifold();
                CT_VTT.OpenAndEvacuate(OkPressure);
                ProcessSubStep.End();
            }

            if (!imWasOpened)
            {
                ProcessSubStep.Start($"Evacuate {IM.Name}");
                VacuumSystem.IsolateManifold();
                IM.OpenAndEvacuate(OkPressure);
                ProcessSubStep.End();
            }

            ProcessSubStep.Start($"Join and Evacuate all sections");
            OpenPreparedGRs();
            MC.PathToVacuum?.Open();     // Opens GM, too; avoid closing GR ports
            VTT.PathToVacuum?.Open();
            IM.PathToVacuum?.Open();
            IM_CT.Open();
            VTT_MC.Open();
            ProcessSubStep.End();

            ProcessStep.End();
        }


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
            WaitForStablePressure(CleanPressure);
            InletPort.Close();
            ProcessStep.End();

            ProcessStep.Start($"Transfer CO2 from MC to {InletPort.Name}");
            Alert("Operator Needed", "Put LN on inlet port.");
            Notice.Send("Operator needed", "Almost ready for LN on inlet port.\r\n" +
                "Press Ok to continue, then raise LN onto inlet port tube");

            ProcessSubStep.Start($"Wait for {MC.Name} to warm up a bit.");
            WaitFor(() => mc.Temperature > CO2TransferStartTemperature);
            ProcessSubStep.End();

            VacuumSystem.Isolate();
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

        protected override void MeasureRemainingVolumes()
        {
            Find<VolumeCalibration>("MCP1, MCP2").Calibrate();
            Find<VolumeCalibration>("Split, GMC").Calibrate();
            Find<VolumeCalibration>("GML").Calibrate();
            Find<VolumeCalibration>("GMR").Calibrate();
            Find<VolumeCalibration>("VTT, CT, IM").Calibrate();
            Find<VolumeCalibration>("VM").Calibrate();
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
            CloseAllGRs();
            VacuumSystem.Isolate();
            IM.Open();
            GM.Open();

            IM_CT.Open();
            CT_VTT.Open();
            VTT_MC.Open();
            MC.OpenPorts();
            MC_GM.Open();
            MC.PathToVacuum?.Open();     // Opens GM, too
            VTT.PathToVacuum?.Open();
            IM.PathToVacuum?.Open();
            VacuumSystem.Evacuate();
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
            EventLog.Record($"Admit test: {gasSupply}, target: {pressure:0.###}, stabilized: {gs.Meter.Value:0.###} in {ProcessStep.Elapsed:m':'ss}");
            gs?.Destination?.OpenAndEvacuate();
        }

        protected virtual void TestPressurize(string gasSupply, double pressure)
        {
            var gs = Find<GasSupply>(gasSupply);
            gs?.Destination?.OpenAndEvacuate(OkPressure);
            gs?.Destination?.ClosePorts();
            gs?.Pressurize(pressure);
            WaitSeconds(60);
            EventLog.Record($"Pressurize test: {gasSupply}, target: {pressure:0.###}, stabilized: {gs.Meter.Value:0.###} in {ProcessStep.Elapsed:m':'ss}");
            gs?.Destination?.OpenAndEvacuate();
        }

        protected virtual void TestGasSupplies()
        {
            //TestAdmit("O2.IML", IMO2Pressure);
            TestPressurize("CO2.MC", 75);
            //TestPressurize("CO2.MC", 1000);
            //TestPressurize("He.IMR", 800);
            //TestPressurize("He.MC", 80);
            //TestPressurize("He.GML", 800);
            //TestAdmit("He.GM", 760);
            //TestPressurize("H2.GMR", 100);
            //TestPressurize("H2.GMR", 900);
        }

        protected override void Test()
        {
            //var grs = new List<IHeater>()
            //{
            //    Find<IHeater>("hGR1"),
            //    Find<IHeater>("hGR3"),
            //    Find<IHeater>("hGR5"),
            //    Find<IHeater>("hGR7"),
            //    Find<IHeater>("hGR9"),
            //    Find<IHeater>("hGR11")
            //}.ToArray();
            //PidStepTest(grs);

            //var ips = new List<IInletPort>()
            //{
            //    Find<IInletPort>("IP7"),
            //    Find<IInletPort>("IP9"),
            //    Find<IInletPort>("IP11")
            //};
            //ips.ForEach(ip => ip.QuartzFurnace.TurnOn());
            //WaitMinutes(10);
            //PidStepTest(ips.Select(ip => ip.SampleFurnace).Cast<IHeater>().ToArray());
            //ips.ForEach(ip => ip.QuartzFurnace.TurnOff());

            //CalibrateManualHeaters();
            //var gs = Find<IGasSupply>("He.GM");
            //gs?.Pressurize(760);
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

            //var gs = Find<GasSupply>("H2.GMR");
            //gs.Pressurize(100);
            //gs.Pressurize(900);

            //var gs = Find<GasSupply>("He.GML");
            //gs.Admit(800);
            //WaitSeconds(10);

            //var gs = Find<GasSupply>("He.IML");
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
            Bleed(FirstTrap, FirstTrapBleedPressure);

            // Open flow bypass when conditions allow it without producing an excessive
            // downstream pressure spike. Then wait for the spike to be evacuated.
            ProcessSubStep.Start("Wait for remaining pressure to bleed down");
            WaitFor(() => IM.Pressure - FirstTrap.Pressure < FirstTrapFlowBypassPressure);
            FirstTrap.Open();   // open bypass if available
            WaitFor(() => FirstTrap.Pressure < FirstTrapEndPressure);
            ProcessSubStep.End();


            //VolumeCalibrations["GR1, GR2"]?.Calibrate();
            //return;
        }

        #endregion Test functions

    }
}