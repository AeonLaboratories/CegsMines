using HACS.Components;
using HACS.Core;
using Microsoft.Win32;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Documents;
using Utilities;
using static Utilities.Utility;

namespace Cegs12X
{
    public partial class Hacs12X
    {

        public void CalibrateManualHeater(IHeater h, IThermocouple tc)
        {
            var minute = 60000;
            double pvTarget = 830;

            if (!h.Config.ManualMode)
            {
                Announce("Invalid heater selected.", $"{h.Name} is not a manual-mode heater.");
                return;
            }

            if (tc.Temperature > pvTarget - 50)
            {
                ProcessStep.Start($"Waiting for calibration thermocouple to cool below {pvTarget - 50:0} °C");
                if (!WaitFor(() => tc.Temperature < pvTarget - 50, minute, 1000))
                {
                    Announce("Error", "Calibration thermocouple is too hot.");
                    ProcessStep.End();
                    return;
                }
                ProcessStep.End();
            }

            if (!Notice.Ok("Operator Needed", $"Move the calibration thermocouple to {h.Name}."))
                return;

            ProcessStep.Start($"Calibrate {h.Name}");

            ProcessSubStep.Start("Pre-heat furnace");
            h.PowerLevel = Math.Min(15, h.Config.MaximumPowerLevel);
            h.TurnOn();
            if (!(WaitFor(() => tc.Temperature > 100, minute, 1000)))
            {
                h.TurnOff();
                Pause($"{h.Name} calibration aborted", $"Temperature isn't rising fast enough. Is the calibration thermocouple in {h.Name}?");
                return;
            }
            WaitFor(() => tc.Temperature > pvTarget - 10, 5 * minute, 1000);
            ProcessSubStep.End();

            h.PowerLevel = 7;
            double delta = 3;           // for a first guess of 10

            while (!Stopping && Math.Abs(delta) > 0)
            {
                var co = h.Config.PowerLevel + delta;
                if (co > h.MaximumPowerLevel)
                {
                    Abort($"{h.Name} calibration stopped", $"{h.Name}: co ({co:0.00}) > MaxPowerLevel {h.MaximumPowerLevel:0.00}.");
                    break;
                }

                h.PowerLevel = h.Config.PowerLevel + delta;
                SampleLog.Record($"{h.Name} calibration: {tc.Temperature:0.00} °C: setting PowerLevel to {h.Config.PowerLevel:0.00}.");

                double tooHigh = 10;
                double tooLong = 2;
                double error = 0;
                bool changeNeeded()
                {
                    if (tc.Temperature > 1000) return true;
                    error = tc.Temperature - pvTarget;
                    ProcessSubStep.CurrentStep.Description =
                        $"{h.Config.PowerLevel:0.00}: {tc.Temperature:0.00} error={error:0.0}";
                    return ProcessSubStep.Elapsed.TotalMinutes > 1 && Math.Abs(error) > tooHigh;
                }

                ProcessSubStep.Start($"PowerLevel = {h.Config.PowerLevel:0.00}; delta = {delta:0.00}; waiting to adjust.");
                WaitFor(changeNeeded, (int)(tooLong * minute), 1000);
                ProcessSubStep.End();

                if (tc.Temperature > 1000)
                {
                    Abort($"{h.Name} calibration stopped", $"Temperature exceeded 1000 °C.");
                    break;
                }

                delta = -error * 0.01;
                if (delta >= 0 && delta < 0.01) delta = 0;
                if (delta < 0 && delta > -0.01) delta = -0.01;
                if (delta > 3) delta = 3;
                if (delta < -3) delta = -3;
            }
            h.TurnOff();

            if (delta == 0)
            {
                SampleLog.Record($"{h.Name} calibration complete: PowerLevel is {h.Config.PowerLevel:0.00}.");
                Alert($"Calibration complete", $"{h.Name}'s PowerLevel is {h.Config.PowerLevel:0.00}.");
            }
            else
            {
                Pause($"{h.Name} calibration unsuccessful", $"Check the sample data log for details.");
            }
            ProcessStep.End();

            void Abort(string caption, string message)
            {
                Alert(caption, message);
                SampleLog.Record(caption + ". " + message);
            }
        }

        public void PidStepTest(params IHeater[] heaters)
        {
            double initialCO = 1.0;
            double stepCO = 4.0;
            int warmup = 60; // seconds
            Array.ForEach(heaters, h => { h.Manual(stepCO); h.TurnOn(); });

            WaitSeconds(warmup); // Preheat furnace.

            Array.ForEach(heaters, h => h.PowerLevel = initialCO);

            WaitMinutes(15); // Wait for temperature to stabalize.
            var names = new List<string>();
            Array.ForEach(heaters, h => names.Add(h.Name));
            var log = new DataLog($"PID Step test{(heaters.Length > 1 ? "s" : "")} {string.Join(',', names)}.txt")
            {
                //Header = string.Join('\t', heaters as IEnumerable),
                ChangeTimeoutMilliseconds = 3 * 1000,
                OnlyLogWhenChanged = false,
                InsertLatestSkippedEntry = false
            };
            var columns = new ObservableList<DataLog.Column>();
            Array.ForEach(heaters, h =>
            {
                columns.Add(new DataLog.Column()
                {
                    Name = h.Name,
                    Resolution = -1.0,
                    Format = "0.00"
                });
            });
            log.Columns = columns;
            HacsLog.List.Add(log);

            Array.ForEach(heaters, h => h.PowerLevel = stepCO);

            WaitMinutes(3);

            log.ChangeTimeoutMilliseconds = 30 * 1000;

            WaitMinutes(17);

            HacsLog.List.Remove(log);
            log.Close();
            log = null;

            Array.ForEach(heaters, h => h.TurnOff());
        }
    }
}
