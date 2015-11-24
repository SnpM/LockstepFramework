using UnityEngine;
using System.Collections;

public enum InputCode : byte
{
	//S for Special
	S1,
	S2,
	S3,
	S4,

	//C for Core
	C1, //Attack
	C2, //Stop
	C3, //Move
	C4,

	//M for Miscellaneous
	M1,
	M2,
	M3,
	M4,

	Build,
	Spawn,
	Meta,
	None,

    Test,
}