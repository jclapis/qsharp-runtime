// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Quantum.Simulation.Core;
using System;
using System.Text;
using SysMath = System.Math;

namespace Microsoft.Quantum.Simulation.Simulators
{
    public partial class QuantumSimulator
    {
        public class DiracDumper : StateDumper
        {
            protected Action<string> Write { get; }

            protected long Precision { get; }

            protected double ZeroTolerance { get; }

            protected bool UseRelativePhases { get; }

            protected string AmplitudeFormatString { get; }

            protected StringBuilder KetBuilder { get; }

            protected bool IsFirstState;

            protected double FirstPhaseRealMultiplier;

            protected double FirstPhaseImaginaryMultiplier;

            protected int NumberOfQubits;

            protected int RemainingStates;

            public DiracDumper(QuantumSimulator Simulator, Action<string> Write, long Precision, double ZeroTolerance, bool UseRelativePhases)
                : base(Simulator)
            {
                this.Write = Write;
                this.Precision = Precision;
                this.ZeroTolerance = ZeroTolerance;
                this.UseRelativePhases = UseRelativePhases;
                KetBuilder = new StringBuilder();

                AmplitudeFormatString = "0.";
                for(int i = 0; i < Precision; i++)
                {
                    AmplitudeFormatString += "#";
                }
            }

            public override bool Dump(IQArray<Qubit> Qubits = null)
            {
                IsFirstState = true;
                KetBuilder.Clear();

                if(Qubits == null)
                {
                    NumberOfQubits = (int)Simulator.QubitManager.GetAllocatedQubitsCount();
                    RemainingStates = (int)SysMath.Pow(2, NumberOfQubits);
                    sim_Dump(Simulator.Id, Callback);
                }
                else
                {
                    NumberOfQubits = (int)Qubits.Length;
                    RemainingStates = (int)SysMath.Pow(2, NumberOfQubits);
                    uint[] qubitIDs = Qubits.GetIds();
                    sim_DumpQubits(Simulator.Id, (uint)NumberOfQubits, qubitIDs, Callback);
                }

                return true;
            }

            public override bool Callback(uint State, double RealComponent, double ImaginaryComponent)
            {
                RemainingStates--;

                // Ignore this state if both the real and imaginary amplitudes are too low
                if (RealComponent < ZeroTolerance &&
                    ImaginaryComponent < ZeroTolerance)
                {
                    if(RemainingStates == 0)
                    {
                        Write(KetBuilder.ToString());
                    }
                    return true;
                }

                // Get the amplitudes for this state relative to the first nonzero state
                if(UseRelativePhases)
                {
                    if (IsFirstState)
                    {
                        double phaseAngle = SysMath.Atan2(ImaginaryComponent, RealComponent);
                        FirstPhaseRealMultiplier = SysMath.Cos(phaseAngle);
                        FirstPhaseImaginaryMultiplier = SysMath.Sin(phaseAngle);
                    }

                    double newReal = RealComponent * FirstPhaseRealMultiplier + 
                        ImaginaryComponent * FirstPhaseImaginaryMultiplier;
                    ImaginaryComponent = ImaginaryComponent * FirstPhaseRealMultiplier -
                        RealComponent * FirstPhaseImaginaryMultiplier;
                    RealComponent = newReal;
                }

                // Get the amplitude string, its sign, and the ket for this state
                (bool amplitudeSign, string amplitudeString) = GetAmplitudeString(RealComponent, ImaginaryComponent);
                string stateKet = GetStateKet(State, NumberOfQubits);

                // Append the sign of the state
                if (IsFirstState)
                {
                    IsFirstState = false;
                }
                else
                {
                    KetBuilder.Append($" {(amplitudeSign ? "+" : "–")} ");
                }

                // Append the amplitude and the ket
                if (amplitudeString == "1")
                {
                    KetBuilder.Append(stateKet);
                }
                else
                {
                    KetBuilder.Append($"{amplitudeString}{stateKet}");
                }

                if (RemainingStates == 0)
                {
                    Write(KetBuilder.ToString());
                }
                return true;
            }

            protected string GetStateKet(uint State, int NumberOfQubits)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("|");
                int reducedState = (int)State;
                for(int i = 0; i < NumberOfQubits; i++)
                {
                    builder.Append(reducedState % 2 == 1 ? "1" : "0");
                    reducedState /= 2;
                }
                builder.Append("⟩");
                return builder.ToString();
            }

            protected (bool, string) GetAmplitudeString(double Real, double Imaginary)
            {
                double realMagnitude = SysMath.Abs(Real);
                double imaginaryMagnitude = SysMath.Abs(Imaginary);

                // If the imaginary amplitude is 0, just return the real amplitude.
                if (imaginaryMagnitude < ZeroTolerance)
                {
                    bool isPositive = (Real > 0.0);
                    string amplitudeString = realMagnitude.ToString(AmplitudeFormatString);
                    return (isPositive, amplitudeString);
                }

                // If the real amplitude is 0, just return the imaginary amplitude.
                else if(realMagnitude < ZeroTolerance)
                {
                    bool isPositive = (Imaginary > 0.0);
                    string amplitudeString = imaginaryMagnitude.ToString(AmplitudeFormatString) + "i";
                    return (isPositive, amplitudeString);
                }

                // If both components are non-zero, build a string for both terms.
                else
                {
                    bool isRealPositive = (Real > 0.0);
                    bool isImaginaryPositive = (Imaginary > 0.0);

                    // Get the imaginary separator based on the parity of the real and imaginary states
                    string imaginarySignString = "+";
                    if (isRealPositive != isImaginaryPositive)
                    {
                        imaginarySignString = "–";
                    }

                    string realString = realMagnitude.ToString(AmplitudeFormatString);
                    string imaginaryString = imaginaryMagnitude.ToString(AmplitudeFormatString);
                    string amplitudeString = $"({realString} {imaginarySignString} {imaginaryString}i)";

                    return (isRealPositive, amplitudeString);
                }
            }

        }
    }
}
