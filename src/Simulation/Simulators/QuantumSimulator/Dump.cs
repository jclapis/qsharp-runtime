// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Quantum.Simulation.Core;

namespace Microsoft.Quantum.Simulation.Simulators
{
    public partial class QuantumSimulator
    {
        #region Dump Method Implementation

        /// <summary>
        /// Dumps the wave function for the given qubits into the given target. 
        /// If the target is QVoid or an empty string, it dumps it to the console
        /// using the `Message` function, otherwise it dumps the content into a file
        /// with the given name.
        /// If the given qubits is null, it dumps the entire wave function, otherwise
        /// it attemps to create the wave function or the resulting subsystem; if it fails
        /// because the qubits are entangled with some external qubit, it just generates a message.
        /// </summary>
        protected virtual QVoid Dump<T>(T target, IQArray<Qubit> qubits = null)
        {
            var filename = (target is QVoid) ? "" : target.ToString();

            QVoid process(Action<string> channel)
            {
                var ids = qubits?.Select(q => (uint)q.Id).ToArray() ?? QubitIds;

                var dumper = new SimpleDumper(this, channel);
                channel($"# wave function for qubits with ids (least to most significant): {string.Join(";", ids)}");

                if (!dumper.Dump(qubits))
                {
                    channel("## Qubits were entangled with an external qubit. Cannot dump corresponding wave function. ##");
                }

                return QVoid.Instance;
            }

            var logMessage = this.Get<ICallable<string, QVoid>, Microsoft.Quantum.Intrinsic.Message>();

            // If no file provided, use `Message` to generate the message into the console;
            if (string.IsNullOrWhiteSpace(filename))
            {
                var op = this.Get<ICallable<string, QVoid>, Microsoft.Quantum.Intrinsic.Message>();
                return process((msg) => op.Apply(msg));
            }
            else
            {
                try
                {
                    using (var file = new StreamWriter(filename))
                    {
                        return process(file.WriteLine);
                    }
                }
                catch (Exception e)
                {
                    logMessage.Apply($"[warning] Unable to write state to '{filename}' ({e.Message})");
                    return QVoid.Instance;
                }
            }
        }

        /// <summary>
        /// Prints the state vector of the provided qubit register in Dirac (ket) notation to the
        /// specified location. States with zero amplitude will be ignored. The location that the state
        /// is printed to depends on the target machine. In most cases, you can either use the empty
        /// tuple ("()") to print it to the console with the <see cref="Intrinsic.Message"/> method, or
        /// use a string representing the path of a logging file on the filesystem.
        /// 
        /// For this method to succeed, the qubits in this register may not be entangled with external
        /// qubits that are not included in the register. Otherwise, it will simply print that the
        /// qubits are entangled and do not have an independent state.
        /// 
        /// If <paramref name="Qubits"/> is null, it will print the state vector of the entire machine
        /// (including all of the qubits that have been allocated).
        /// 
        /// Each ket in the state vector will be provided in little-endian form. 
        /// </summary>
        protected virtual QVoid DumpDirac<T>(T Location, long Precision, double ZeroTolerance, bool UseRelativePhases, IQArray<Qubit> Qubits = null)
        {
            string target = (Location is QVoid) ? "" : Location.ToString();
            StreamWriter writer = null;
            try
            {
                ICallable<string, QVoid> logMessage = Get<ICallable<string, QVoid>, Intrinsic.Message>();
                Action<string> writerDelegate = (message) => logMessage.Apply(message);
                if(!string.IsNullOrWhiteSpace(target))
                {
                    writer = new StreamWriter(target);
                    writerDelegate = writer.Write;
                }

                DiracDumper dumper = new DiracDumper(this, writerDelegate, Precision, ZeroTolerance, UseRelativePhases);
                var ids = Qubits?.Select(q => (uint)q.Id).ToArray() ?? QubitIds;
                writerDelegate($"# wave function for qubits with ids (least to most significant): {string.Join(";", ids)}");

                if (!dumper.Dump(Qubits))
                {
                    writerDelegate($"## Qubits were entangled with an external qubit. Cannot dump corresponding wave function. ##");
                }

            }
            catch(Exception ex)
            {
                var logMessage = this.Get<ICallable<string, QVoid>, Microsoft.Quantum.Intrinsic.Message>();
                logMessage.Apply($"[warning] Unable to write state to '{target}': ({ex.Message})");
            }
            finally
            {
                writer?.Dispose();
            }

            return QVoid.Instance;
        }

        #endregion

        #region QS ICallable Implementations

        public class QsimDumpMachine<T> : Quantum.Diagnostics.DumpMachine<T>
        {
            private QuantumSimulator Simulator { get; }


            public QsimDumpMachine(QuantumSimulator m) : base(m)
            {
                this.Simulator = m;
            }

            public override Func<T, QVoid> Body => (location) =>
            {
                if (location == null) { throw new ArgumentNullException(nameof(location)); }

                return Simulator.Dump(location);
            };
        }

        public class QSimDumpRegister<T> : Quantum.Diagnostics.DumpRegister<T>
        {
            private QuantumSimulator Simulator { get; }


            public QSimDumpRegister(QuantumSimulator m) : base(m)
            {
                this.Simulator = m;
            }

            public override Func<(T, IQArray<Qubit>), QVoid> Body => (__in) =>
            {
                var (location, qubits) = __in;

                if (location == null) { throw new ArgumentNullException(nameof(location)); }
                Simulator.CheckQubits(qubits);

                return Simulator.Dump(location, qubits);
            };
        }

        /// <summary>
        /// Implements the <see cref="Diagnostics.DumpRegisterDirac{__T__}"/> method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class QSimDumpRegisterDirac<T> : Diagnostics.DumpRegisterDirac<T>
        {
            /// <summary>
            /// The simulator that's currently executing
            /// </summary>
            private readonly QuantumSimulator Simulator;

            /// <summary>
            /// Creates a new <see cref="QSimDumpRegisterDirac{T}"/> instance.
            /// </summary>
            /// <param name="TargetMachine">The simulator that's currently executing</param>
            public QSimDumpRegisterDirac(QuantumSimulator TargetMachine)
                : base(TargetMachine)
            {
                Simulator = TargetMachine;
            }

            /// <summary>
            /// Provides the actual functionality of the method.
            /// </summary>
            public override Func<(T, IQArray<Qubit>, long, double, bool), QVoid> Body => (argumentTuple) =>
            {
                var (location, qubits, precision, zeroTolerance, useRelativePhases) = argumentTuple;

                // Validation checks
                if (location == null)
                {
                    throw new ArgumentNullException(nameof(location));
                }
                Simulator.CheckQubits(qubits); // Make sure qubits is not null and not empty

                return Simulator.DumpDirac(location, precision, zeroTolerance, useRelativePhases, qubits);
            };
        }

        #endregion
    }
}
