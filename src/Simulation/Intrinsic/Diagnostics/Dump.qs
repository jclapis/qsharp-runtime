// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Quantum.Diagnostics {

    /// # Summary
    /// Dumps the current target machine's status.
    ///
    /// # Input
    /// ## location
    /// Provides information on where to generate the machine's dump.
    /// See the Remarks section for more details.
    ///
    /// # Output
    /// None.
    ///
    /// # Remarks
    /// This method allows you to dump information about the current status of the
    /// target machine into a file or some other location.
    /// The actual information generated and the semantics of `location`
    /// are specific to each target machine. However, providing an empty tuple as a location (`()`)
    /// or just omitting the `location` parameter typically means to generate the output to the console.
    ///
    /// For the local full state simulator distributed as part of the
    /// Quantum Development Kit, this method expects a string with
    /// the path to a file in which it will write the wave function as a
    /// one-dimensional array of complex numbers, in which each element represents
    /// the amplitudes of the probability of measuring the corresponding state.
    function DumpMachine<'T> (location : 'T) : Unit {
    }

    /// # Summary
    /// Dumps the current target machine's status associated with the given qubits.
    ///
    /// # Input
    /// ## location
    /// Provides information on where to generate the state's dump.
    /// See the Remarks section for more details.
    /// ## qubits
    /// The list of qubits to report.
    ///
    /// # Output
    /// None.
    ///
    /// # Remarks
    /// This method allows you to dump the information associated with the state of the
    /// given qubits into a file or some other location.
    /// The actual information generated and the semantics of `location`
    /// are specific to each target machine. However, providing an empty tuple as a location (`()`)
    /// typically means to generate the output to the console.
    ///
    /// For the local full state simulator distributed as part of the
    /// Quantum Development Kit, this method expects a string with
    /// the path to a file in which it will write the
    /// state of the given qubits (i.e. the wave function of the corresponding subsystem) as a
    /// one-dimensional array of complex numbers, in which each element represents
    /// the amplitudes of the probability of measuring the corresponding state.
    /// If the given qubits are entangled with some other qubit and their
    /// state can't be separated, it just reports that the qubits are entangled.
    /// 
    /// The state of each output element will be provided in little-endian form. To output
    /// big-endian form instead, use the `BigEndianAsLittleEndian` function to pass in the register
    /// with the qubit order reversed.
    /// 
    /// # See Also
    /// - Microsoft.Quantum.Arithmetic.BigEndianAsLittleEndian
    function DumpRegister<'T> (location : 'T, qubits : Qubit[]) : Unit {
    }

	/// # Summary
	/// Prints the state vector of the provided qubit register in Dirac (ket) notation to the
	/// specified location. States with zero amplitude will be ignored.
	/// 
	/// # Input
	/// ## Location
	/// The location to dump the register's state vector. See the Remarks section for more details.
	/// 
	/// ## Qubits
	/// The qubit register to retrieve the state vector for.
	/// 
	/// ## Precision
	/// The number of decimal places to use when displaying a state's amplitude.
	/// 
	/// ## ZeroTolerance
	/// States with amplitudes smaller than this amount will be considered to have zero amplitude
	/// and ignored from the state vector. This is used to handle floating-point errors. The real
	/// and imaginary amplitudes will be compared to this value independently. You can use exponent
	/// notation (for example, `1E-17`) for very small tolerances.
	/// 
	/// ## UseRelativePhases
	/// Set this to true to print the phases of all states relative to the first one (thus ignoring
	/// global phase). Set this to false to print the raw values of each state's amplitude (thus 
	/// retaining global phase information).
	/// 
	/// # Type Parameters
	/// ## 'T
	/// The type of the `Location` parameter.
	/// 
	/// # Remarks
	/// This is a debugging method that allows you to directly observe the state vector of a
	/// qubit register without measuring them. It only works when the target machine is a classical
	/// simulator. The location that the state is printed to depends on the target machine. In most
	/// cases, you can either use the empty tuple (`()`) to print it to the console with the
	/// `Message` method, or use a `String` representing the path of a logging file on the filesystem.
	/// 
	/// For this method to succeed, the qubits in this register may not be entangled with external
	/// qubits that are not included in the register. Otherwise, it will simply print that the
	/// qubits are entangled and do not have an independent state.
    /// 
    /// Each ket in the state vector will be provided in little-endian form. To output big-endian
    /// form instead, use the `BigEndianAsLittleEndian` function to pass in the register with the
    /// qubit order reversed.
	function DumpRegisterDirac<'T>(
		Location : 'T, 
		Qubits : Qubit[], 
		Precision : Int, 
		ZeroTolerance : Double,
		UseRelativePhases : Bool
	) : Unit
	{
		
	}

}
