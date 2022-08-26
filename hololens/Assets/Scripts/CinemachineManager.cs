#if !UNITY_WSA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// editor
//using UnityEditor.Animations;

//using Cinemachine.Utility;
using Cinemachine;
//using Cinemachine.Editor;
//using UnityEditor.SceneManagement;
//using UnityEngine.SceneManagement;




public class CinemachineManager : MonoBehaviour
{
    public CinemachineStateDrivenCamera stateDrivenCamera;
    //public Camera[] cameraThatAreNotViewPoint;
    public GameObject sceneCenter;

    private List<Camera> existingCamera;
    //public List<int> animationsHash;
    private bool hasNotChanged;
    public bool go = false;

    public bool forceRefresh = false;

    //private AnimatorController controller;
    public RuntimeAnimatorController controller;
    
    private string currentCameraName;
    private int i = 0;

    public List<Camera> cameraFiloStack;
    //public float defaultFOV = 54;

    public GameObject displayedCamera;

    int cam_hash_main = 2131779971;
    int cam_hash_kinect = -2016924918;
    int cam_hash_hololens = -1973899216;

    public Camera virtualCamera;
    private CinemachineVirtualCamera cinemachineVirtualCamera;


    int[] cam_hash =
    {
        2131779971,
        -2016924918,
        -1973899216,
        -739893810,
        1257196660,
        1038646498,
        -1551046335,
        -729032233,
        1300400237,
        981842171,
         -1438997142,
         -583166468,
         -1999317365,
         -2890211,
         1725609895,
         299730737,
         -1883696494,
         -121757180,
         1639412670,
         380650280,
         -2045813063,
         -251105745,
         -1543928504,
         -721504802
    };

    void Start()
    {
        existingCamera = new List<Camera>();
        cameraFiloStack = new List<Camera>();
    }

    public CinemachineVirtualCamera GetMainVirtualCamera()
    {
        return cinemachineVirtualCamera;
    }

    void ListExistingCameras()
    {
        if (stateDrivenCamera.LiveChild != null)
            currentCameraName = stateDrivenCamera.LiveChild.VirtualCameraGameObject.name;

        Camera[] cameras = Resources.FindObjectsOfTypeAll(typeof(Camera)) as Camera[];
        List<Camera> tmpCurrentExistingCamera = new List<Camera>();
        List<Camera> currentExistingCamera = new List<Camera>();

        for (int i = 0; i < cameras.Length; ++i)
            if (cameras[i].gameObject.scene.IsValid() && cameras[i].gameObject.GetComponent<IsViewPoint>() != null)
                tmpCurrentExistingCamera.Add(cameras[i]);

        // update camera filo stack
        for (int i = 0; i < tmpCurrentExistingCamera.Count; ++i)
            if (!cameraFiloStack.Contains(tmpCurrentExistingCamera[i]))
                cameraFiloStack.Add(tmpCurrentExistingCamera[i]);

        // sort cameras
        for (int i = 0; i < cameraFiloStack.Count; ++i)
            if (tmpCurrentExistingCamera.Contains(cameraFiloStack[i]))
                currentExistingCamera.Add(cameraFiloStack[i]);

        hasNotChanged = (currentExistingCamera.Count == existingCamera.Count);

        for (int i = 0; hasNotChanged && i < currentExistingCamera.Count; ++i)
            hasNotChanged &= (currentExistingCamera[i] == existingCamera[i]);

        existingCamera = currentExistingCamera;
    } 

    void UpdateVirtualCamera()
    {
        CinemachineVirtualCamera[] virtualCameras = stateDrivenCamera.GetComponentsInChildren<CinemachineVirtualCamera>();
           
        for (int i = 0; i < virtualCameras.Length; ++i)
            Destroy(virtualCameras[i].gameObject);

        for(int i = 0; i < existingCamera.Count; ++i)
        {
            if (existingCamera[i].GetComponent<IsViewPoint>() == null)
                return;

            GameObject newVirtualCamera = new GameObject(existingCamera[i].name + " - CM Vcam - " + i);
            newVirtualCamera.layer = LayerMask.NameToLayer("StateDrivenCamera");

            MoveWith comp = newVirtualCamera.AddComponent<MoveWith>();
            comp.toFollow = existingCamera[i].gameObject;

            CinemachineVirtualCamera virtualCam = newVirtualCamera.AddComponent<CinemachineVirtualCamera>();
            virtualCam.m_Follow = sceneCenter.transform;
            virtualCam.m_Lens.FieldOfView = existingCamera[i].GetComponent<IsViewPoint>().fov;// defaultFOV;

            if (existingCamera[i] == virtualCamera)
                cinemachineVirtualCamera = virtualCam;

            newVirtualCamera.transform.parent = stateDrivenCamera.transform;
            newVirtualCamera.transform.position = existingCamera[i].transform.position;
            newVirtualCamera.transform.rotation = existingCamera[i].transform.rotation;

            CinemachineTransposer transp = virtualCam.AddCinemachineComponent<CinemachineTransposer>();
            transp.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            transp.m_FollowOffset = existingCamera[i].transform.position;
        }
    }

    void UpdateAnimator()
    {

        string controllerPath = "CinemachineCameraAnimator" + existingCamera.Count;

        controller = Resources.Load<RuntimeAnimatorController>(controllerPath);
        stateDrivenCamera.m_AnimatedTarget.runtimeAnimatorController = controller;

        //controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/Animation/CinemachineCameraAnimator" + existingCamera.Count + ".controller");
        //stateDrivenCamera.m_AnimatedTarget.runtimeAnimatorController = controller;

        //var rootStateMachine = controller.layers[0].stateMachine;

        //List<AnimatorState> states = new List<AnimatorState>();

        //// first state
        //string initialStateName = existingCamera[0].name;
        //var initialState = rootStateMachine.AddState(initialStateName);
        //states.Add(initialState);

        //var lastState = initialState;
        //string currentStateName = initialStateName;

        //string paramName = "GoNextState";
        //controller.AddParameter(paramName, AnimatorControllerParameterType.Trigger);

        //for (int i = 1; i < existingCamera.Count; ++i)
        //{
        //    currentStateName = existingCamera[i].name;

        //    var currentState = rootStateMachine.AddState(currentStateName);
        //    states.Add(currentState);


        //    var transition = lastState.AddTransition(currentState);
        //    transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, paramName);

        //    lastState = currentState;
        //}

        //var finalTransition = lastState.AddTransition(initialState);
        //finalTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, paramName);

        //// redirect state
        //// need to be at the en of this function
        //string redirectStateName = "redirect";
        //var redirectState = rootStateMachine.AddState(redirectStateName);
        //controller.AddParameter("GoRedirectState", AnimatorControllerParameterType.Trigger);


        //for (int i = 0; i < states.Count; ++i)
        //{
        //    var transitionRedirectIn = redirectState.AddTransition(states[i]);
        //    controller.AddParameter("Go" + existingCamera[i].name, AnimatorControllerParameterType.Trigger);
        //    transitionRedirectIn.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Go" + existingCamera[i].name);
        //    var transitionRedirectOut = states[i].AddTransition(redirectState);
        //    transitionRedirectOut.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "GoRedirectState");
        //}
    }

    void UpdateLinks()
    {
        //StateCollector collector = new StateCollector();
        //collector.CollectStates(controller, 0);

        CinemachineStateDrivenCamera.Instruction[] newSet = new CinemachineStateDrivenCamera.Instruction[existingCamera.Count];

        for (int i = 0; i < stateDrivenCamera.ChildCameras.Length; ++i)
        {
            CinemachineStateDrivenCamera.Instruction inst = new CinemachineStateDrivenCamera.Instruction();
            inst.m_VirtualCamera = stateDrivenCamera.ChildCameras[i];
            //inst.m_FullHash = collector.mStates[i+1];

            if (stateDrivenCamera.ChildCameras[i].name.Contains("Main"))
            {
                inst.m_FullHash = cam_hash_main;
                //Debug.Log("main specific hash " + cam_hash_main + " for string : " + stateDrivenCamera.ChildCameras[i].name);
            }
            else if (stateDrivenCamera.ChildCameras[i].name.Contains("Kinect"))
            {
                inst.m_FullHash = cam_hash_kinect;
                //Debug.Log("kinect specific hash " + cam_hash_kinect + " for string : " + stateDrivenCamera.ChildCameras[i].name);
            }
            else if (stateDrivenCamera.ChildCameras[i].name.Contains("Hololens"))
            {
                inst.m_FullHash = cam_hash_hololens;
                //Debug.Log("hololens specific hash " + cam_hash_hololens + " for string : " + stateDrivenCamera.ChildCameras[i].name);
            }
            else
            {
                //Debug.Log("other cam : " + stateDrivenCamera.ChildCameras[i].name);
                inst.m_FullHash = cam_hash[i];
            }

            //Debug.Log("inst.m_FullHash["+i+"] : " + inst.m_FullHash);

            newSet[i] = inst;
        }
        stateDrivenCamera.m_Instructions = newSet;

        // try to update manually GUI
        stateDrivenCamera.enabled = false;
        forceRefresh = true;

        // reset camera
        if (stateDrivenCamera.LiveChild != null && stateDrivenCamera.LiveChild.VirtualCameraGameObject.name != currentCameraName)
        {
            GameObject cam = GameObject.Find(currentCameraName);
            if(cam != null)
            {
                CinemachineVirtualCamera vcam = cam.GetComponent<CinemachineVirtualCamera>();
                if (vcam != null)
                {
                    stateDrivenCamera.LiveChild = vcam;
                }
            }
        }
    }

    void UpdateCinemachine()
    {
        if(!hasNotChanged)
            i = 0;

        if (i == 0)
        {
            UpdateVirtualCamera();
            i++;
        }
        else if (i == 1)
        {
            UpdateAnimator();
            i++;
        }
        else if (i == 2)
        {
            UpdateLinks();
            i = -1;

            if (displayedCamera != null)
            {
                //stateDrivenCamera.LiveChild = displayedCamera;
                string cameraName = displayedCamera.name;
                //Debug.Log("go last state : " + cameraName);

                stateDrivenCamera.m_AnimatedTarget.SetTrigger("GoRedirectState");
                stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + cameraName);
            }
        }
    }

    void Update()
    {
        if (forceRefresh)
        {
            stateDrivenCamera.enabled = true;
            forceRefresh = false;
        }

        ListExistingCameras();

        UpdateCinemachine();

        //Debug.Log("LiveChild > " + stateDrivenCamera.LiveChild);
        if(stateDrivenCamera.LiveChild != null)
            displayedCamera = stateDrivenCamera.LiveChild.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>().gameObject.GetComponent<MoveWith>().toFollow;

        //tateDrivenCamera.LiveChild = displayedCamera;
        //stateDrivenCamera.m_AnimatedTarget.SetTrigger("Go" + displayedCamera.name);
    }










    // this class was copied directly from the script: CinemachineStateDrivenCameraEditor.cs
    // and used on advice from https://forum.unity.com/threads/can-you-add-and-define-a-state-on-a-state-driven-camera-via-c.749063/
    //class StateCollector
    //{
    //    public List<int> mStates;
    //    public List<string> mStateNames;
    //    public Dictionary<int, int> mStateIndexLookup;
    //    public Dictionary<int, int> mStateParentLookup;
    //    public void CollectStates(AnimatorController ac, int layerIndex)
    //    {
    //        mStates = new List<int>();
    //        mStateNames = new List<string>();
    //        mStateIndexLookup = new Dictionary<int, int>();
    //        mStateParentLookup = new Dictionary<int, int>();
    //        mStateIndexLookup[0] = mStates.Count;
    //        mStateNames.Add("(default)");
    //        mStates.Add(0);
    //        if (ac != null && layerIndex >= 0 && layerIndex < ac.layers.Length)
    //        {
    //            AnimatorStateMachine fsm = ac.layers[layerIndex].stateMachine;
    //            string name = fsm.name;
    //            int hash = Animator.StringToHash(name);
    //            CollectStatesFromFSM(fsm, name + ".", hash, string.Empty);
    //        }
    //    }
    //    void CollectStatesFromFSM(
    //        AnimatorStateMachine fsm, string hashPrefix, int parentHash, string displayPrefix)
    //    {
    //        ChildAnimatorState[] states = fsm.states;
    //        for (int i = 0; i < states.Length; i++)
    //        {
    //            AnimatorState state = states[i].state;
    //            int hash = AddState(Animator.StringToHash(hashPrefix + state.name),
    //                parentHash, displayPrefix + state.name);
    //            // Also process clips as pseudo-states, if more than 1 is present.
    //            // Since they don't have hashes, we can manufacture some.
    //            var clips = CollectClips(state.motion);
    //            if (clips.Count > 1)
    //            {
    //                string substatePrefix = displayPrefix + state.name + ".";
    //                foreach (AnimationClip c in clips)
    //                    AddState(
    //                        CinemachineStateDrivenCamera.CreateFakeHash(hash, c),
    //                        hash, substatePrefix + c.name);
    //            }
    //        }
    //        ChildAnimatorStateMachine[] fsmChildren = fsm.stateMachines;
    //        foreach (var child in fsmChildren)
    //        {
    //            string name = hashPrefix + child.stateMachine.name;
    //            string displayName = displayPrefix + child.stateMachine.name;
    //            int hash = AddState(Animator.StringToHash(name), parentHash, displayName);
    //            CollectStatesFromFSM(child.stateMachine, name + ".", hash, displayName + ".");
    //        }
    //    }
    //    List<AnimationClip> CollectClips(Motion motion)
    //    {
    //        var clips = new List<AnimationClip>();
    //        AnimationClip clip = motion as AnimationClip;
    //        if (clip != null)
    //            clips.Add(clip);
    //        BlendTree tree = motion as BlendTree;
    //        if (tree != null)
    //        {
    //            ChildMotion[] children = tree.children;
    //            foreach (var child in children)
    //                clips.AddRange(CollectClips(child.motion));
    //        }
    //        return clips;
    //    }
    //    int AddState(int hash, int parentHash, string displayName)
    //    {
    //        if (parentHash != 0)
    //            mStateParentLookup[hash] = parentHash;
    //        mStateIndexLookup[hash] = mStates.Count;
    //        mStateNames.Add(displayName);
    //        mStates.Add(hash);
    //        return hash;
    //    }
    //}
}
#endif