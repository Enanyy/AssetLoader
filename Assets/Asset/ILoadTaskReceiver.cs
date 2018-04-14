using UnityEngine;
using System.Collections;

public interface ILoadTaskReceiver 
{

	void OnLoadFinish ();
	void OnLoadProcess (int varFinishCount, int varTotal);
}

