using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YourCommonTools;
using YourNetworkingTools;
using UnityEngine.UI;
#if ENABLE_MULTIPLAYER_TIMELINE
using MultiplayerTimeline;
#endif

namespace YourNetworkingTools
{
    /******************************************
	* 
	* ActorTimeline
	* 
	* Extension class of the game's actor for the use of it in Multiplayer Timeline
	* 
	* @author Esteban Gallardo
	*/
    public class ActorTimeline : Actor
    {
        // ----------------------------------------------
        // PUBLIC EVENTS
        // ----------------------------------------------
        public const string EVENT_ACTOR_COLLISION_ENTER     = "EVENT_ACTOR_COLLISION_ENTER";
        public const string EVENT_ACTOR_COLLISION_EXIT      = "EVENT_ACTOR_COLLISION_EXIT";
        public const string EVENT_ACTOR_DEAD                = "EVENT_ACTOR_DEAD";
        public const string EVENT_ACTOR_DESTROYED           = "EVENT_ACTOR_DESTROYED";

        public const string EVENT_ACTORTIMELINE_STATE_FACE_PLAYER   = "EVENT_ACTORTIMELINE_STATE_FACE_PLAYER";
        public const string EVENT_ACTORTIMELINE_GO_ACTION_ENDED     = "EVENT_ACTORTIMELINE_STATE_FACE_PLAYER";
        public const string EVENT_ACTORTIMELINE_WAYPOINT_UPDATED    = "EVENT_ACTORTIMELINE_WAYPOINT_UPDATED";

        public const string EVENT_GAMEPLAYER_SETUP_AVATAR           = "EVENT_GAMEPLAYER_SETUP_AVATAR";
        public const string EVENT_GAMEPLAYER_CREATED_NEW            = "EVENT_GAMEPLAYER_CREATED_NEW";
        public const string EVENT_GAMECHARACTER_NEW_ANIMATION       = "EVENT_GAMECHARACTER_NEW_ANIMATION";
        public const string EVENT_GAMECHARACTER_NEW_STATE           = "EVENT_GAMECHARACTER_NEW_STATE";
        public const string EVENT_GAMEPLAYER_HUMAN_PLAYER_NAME      = "EVENT_GAMEPLAYER_HUMAN_PLAYER_NAME";
        public const string EVENT_GAMEPLAYER_HUMAN_DIRECTOR_NAME    = "EVENT_GAMEPLAYER_HUMAN_DIRECTOR_NAME";
        public const string EVENT_GAMEPLAYER_HUMAN_SPECTATOR_NAME   = "EVENT_GAMEPLAYER_HUMAN_SPECTATOR_NAME";
        public const string EVENT_GAMECHARACTER_POSITION_LOCAL_PLAYER="EVENT_GAMECHARACTER_POSITION_LOCAL_PLAYER";

        public const string EVENT_GAMEPLAYER_DATA_POSITION_PLAYER = "EVENT_GAMEPLAYER_DATA_POSITION_PLAYER";

        public const string EVENT_GAMEPLAYER_REAL_FORWARD           = "EVENT_GAMEPLAYER_REAL_FORWARD";

        public const string LAYER_PLAYERS = "PLAYERS";
        public const string LAYER_ENEMIES = "ENEMIES";
        public const string LAYER_NPCS    = "NPCS";
        public const string LAYER_ITEMS   = "ITEMS";

        public const char SEPARATOR_ANIMATION_ENTRY = '#';
        public const char SEPARATOR_PARAMS_ANIMATION = ';';

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const int ANIMATION_IDLE = 0;
        public const int ANIMATION_WALK = 1;

        public const float DISTANCE_TO_UPDATE_TO_NEXT_WAYPOINT = 1f;

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public string NameActor = "";
        public string ClassName = "";
        public bool EnableAutoInitialization = false;
        public bool EnableTriggerCollision = true;
        public GameObject[] ModelStateGO;

        // ----------------------------------------------
        // PROTECTED MEMBERS
        // ----------------------------------------------
        protected List<GameObject> m_modelStates = new List<GameObject>();
        protected string m_modelState;
        protected int m_modelIndex;
        protected int m_modelAnimation;
        protected Vector3 m_initialPosition;

        protected GameObject m_playerToFace = null;

        protected Vector3 m_targetPath = Vector3.down;
        protected List<Vector3> m_pathWaypoints;
        protected int m_currentPathWaypoint;
        protected bool m_goToPosition = false;
        protected string[] m_masksToIgnore;

        protected string m_initialData = null;
        protected Text m_txtLife;

        protected GameObject m_ghostPlayer;
        protected Vector3 m_positionLocalPlayer = Vector3.zero;
        protected float m_timeForGhost = 0;

        protected Dictionary<string, int> m_customAnimations = new Dictionary<string, int>();

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------
        public string Name
        {
            get { return NameActor; }
            set { NameActor = value; }
        }
        public override float Life
        {
            get { return m_life; }
            set
            {
                if ((value <= 0) && (m_life > 0))
                {
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTOR_DEAD, Name);
                }
                m_life = value;
            }
        }
        public string ModelState
        {
            get { return m_modelState; }
            set
            {
                m_modelState = value;

                bool existsState = false;
                for (int i = 0; i < m_modelStates.Count; i++)
                {
                    if (m_modelStates[i].name.Equals(m_modelState))
                    {
                        existsState = true;
                    }
                }

                if (existsState)
                {
                    for (int i = 0; i < m_modelStates.Count; i++)
                    {
                        if (m_modelStates[i].name.Equals(m_modelState))
                        {
                            m_modelStates[i].SetActive(true);
                            m_modelIndex = i;
                        }
                        else
                        {
                            m_modelStates[i].SetActive(false);
                        }
                    }
                }
            }
        }
        public int ModelIndex
        {
            get { return m_modelIndex; }
            set
            {
                m_modelIndex = value;
                if (m_modelIndex > m_modelStates.Count)
                {
                    m_modelIndex = m_modelIndex % m_modelStates.Count;
                }
                if (m_modelIndex >= 0)
                {
                    for (int i = 0; i < m_modelStates.Count; i++)
                    {
                        if (m_modelStates[i].name.Equals(m_modelState))
                        {
                            m_modelStates[i].SetActive(false);
                        }
                    }
                    m_modelStates[m_modelIndex].SetActive(true);
                }
            }
        }
        public virtual int ModelAnimation
        {
            get { return m_modelAnimation; }
            set
            {
                if (value >= 0)
                {
                    m_modelAnimation = value;
                    if (m_modelIndex < m_modelStates.Count)
                    {
                        GameObject subModel = m_modelStates[m_modelIndex];
                        if (subModel != null)
                        {
                            Animator animatorSubModel = subModel.GetComponent<Animator>();
                            if (animatorSubModel != null)
                            {
                                animatorSubModel.SetInteger("stateID", m_modelAnimation);
                            }
                        }
                    }
                }
            }
        }
        public NetworkID NetworkID
        {
            get {
                if (this.gameObject.GetComponent<ActorNetwork>() != null)
                {
                    return this.gameObject.GetComponent<ActorNetwork>().NetworkID;
                }
                else
                {
                    return null;
                }                    
            }
        }
        public string EventNameObjectCreated
        {
            set {
                if (this.gameObject.GetComponent<ActorNetwork>() != null)
                {
                    this.gameObject.GetComponent<ActorNetwork>().EventNameObjectCreated = value;
                }                
            }
        }
        public bool IsMine()
        {
            if (this.gameObject.GetComponent<ActorNetwork>() == null)
            {
                return true;
            }
            else
            {
                return this.gameObject.GetComponent<ActorNetwork>().IsMine();
            }            
        }
        public virtual bool EnableBackgroundVR
        {
            get { return true; }
        }
        public virtual bool DirectorMode
        {
            get { return false; }
        }
        public virtual float DISTANCE_TO_ACTIVATE_GHOST
        {
            get { return 3f; }
        }

        // -------------------------------------------
        /* 
		 * Initialization of the element
		 */
        public override void Start()
        {
            base.Start();

            if (EnableAutoInitialization)
            {
                if (NameActor.Length == 0) NameActor = this.gameObject.name;
                GetModel();
            }
        }

        // -------------------------------------------
        /* 
		 * Destroy the current object
		 */
        public override bool Destroy()
        {
            if (base.Destroy()) return true;

            NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
#if ENABLE_MULTIPLAYER_TIMELINE
            TimelineEventController.Instance.TimelineEvent -= OnTimelineEvent;
#endif

            if (m_ghostPlayer != null)
            {
                BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
                GameObject.Destroy(m_ghostPlayer);
                m_ghostPlayer = null;
            }            

            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTOR_DESTROYED, ClassName, Name);
            return false;
        }

        // -------------------------------------------
        /* 
		* Initialize
		*/
        public override void Initialize(params object[] _list)
        {
            if (_list != null)
            {
                if (_list.Length > 0)
                {
                    if (_list[0] != null)
                    {
                        if (_list[0] is string)
                        {
                            if (m_initialData == null)
                            {
                                InitializeWithData((string)_list[0]);
                            }
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		* InitializeLocalData
		*/
        public void InitializeLocalData(string _initialData)
        {
#if ENABLE_CONFUSION
            InitializeWithData(_initialData);
#endif
        }

        // -------------------------------------------
        /* 
		* InitializeWithData
		*/
        protected virtual void InitializeWithData(string _initialData)
        {
            m_initialData = _initialData;
            string[] initialData = m_initialData.Split(',');
            m_initialPosition = new Vector3(float.Parse(initialData[2]), float.Parse(initialData[3]), float.Parse(initialData[4]));
            transform.position = m_initialPosition;
            Name = initialData[0];
            ClassName = initialData[1];
            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_GAMEPLAYER_HUMAN_PLAYER_NAME, Name, ClassName);
            this.gameObject.layer = LayerMask.NameToLayer(LAYER_PLAYERS);
            if (IsMine())
            {
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_GAMEPLAYER_SETUP_AVATAR, this.gameObject);
                NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA, NetworkID.GetID(), m_initialData);
            }
            InitializeCommon();
            if (initialData.Length > 5)
            {
                InitializeCustomAnimations(initialData[5]);
            }
        }

        // -------------------------------------------
        /* 
		* InitializeCustomAnimations
		*/
        protected void InitializeCustomAnimations(string _data)
        {
            m_customAnimations = new Dictionary<string, int>();
            string[] anims = _data.Split(SEPARATOR_ANIMATION_ENTRY);
            for (int i = 0; i < anims.Length; i++)
            {
                if (anims[i].IndexOf(SEPARATOR_PARAMS_ANIMATION) != -1)
                {
                    string[] parmsAnim = anims[i].Split(SEPARATOR_PARAMS_ANIMATION);
                    string nameAnimation = parmsAnim[0];
                    int indexAnimation = int.Parse(parmsAnim[1]);
                    m_customAnimations.Add(nameAnimation, indexAnimation);
                }
            }
        }

        // -------------------------------------------
        /* 
		* InitializeCommon
		*/
        public virtual void InitializeCommon()
        {
            m_life = 100;
            if (m_model == null)
            {
                Debug.LogError("CREATE AN INSTANCE OF GAMEPLAYER[" + ClassName + "]!!!!!!!!!!!!!!!!!!!!!");
                GameObject humanPlayer = Utilities.AttachChild(this.gameObject.transform, AssetbundleController.Instance.CreateGameObject(ClassName));
                humanPlayer.name = "Model";
                humanPlayer.transform.position -= new Vector3(0, 1.4f, 0);

                if (this.transform.Find("Canvas/Life") != null)
                {
                    m_txtLife = this.transform.Find("Canvas/Life").GetComponent<Text>();
                    m_txtLife.text = m_life.ToString();
                }

                // ANIMATION STATES
                if (GetModel() != null)
                {
                    NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
#if ENABLE_MULTIPLAYER_TIMELINE
                    TimelineEventController.Instance.TimelineEvent += new TimelineEventHandler(OnTimelineEvent);
#endif

                    // ANIMATIONS
                    CreateAnimationStates("stateID,0",   // IDLE
                                          "stateID,1"    // WALK
                                            );

                    if (IsMine())
                    {
                        if (m_model != null) m_model.SetActive(false);
                    }
                    else
                    {
                        if (!EnableBackgroundVR)
                        {
                            if (m_model != null) m_model.SetActive(false);
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------
        /**
		 * Will link to the internal 3D model
		 */
        public override Transform GetModel()
        {
            if (m_model == null)
            {
                if (transform.Find("Model") != null)
                {
                    m_model = transform.Find("Model").gameObject;
                }
                if (m_model != null)
                {
                    if (transform.childCount > 0)
                    {
                        if (transform.GetChild(0).Find("Model") != null)
                        {
                            m_model = transform.GetChild(0).Find("Model").gameObject;
                        }
                    }
                }

                if (m_model != null)
                {
                    if (m_model.transform.childCount == 1)
                    {
                        m_modelState = "";
                    }
                    else
                    {
                        InitializeModelStates();
                    }
                }
            }
            if (m_model != null)
            {
                return m_model.transform;
            }
            else
            {
                return null;
            }
        }

        // ---------------------------------------------------
        /**
		 * InitializeModelStates
		 */
        protected void InitializeModelStates()
        {
            if (ModelStateGO.Length > 0)
            {
                for (int i = 0; i < ModelStateGO.Length; i++)
                {
                    GameObject submodel = ModelStateGO[i];
                    m_modelStates.Add(submodel);
                    if (submodel.activeSelf)
                    {
                        m_modelState = ModelStateGO[i].name;
                        m_modelIndex = i;
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * OnCollisionEnter
		 */
        public virtual void OnCollisionEnter(Collision _collision)
        {
            // Debug.LogError("Actor::OnCollisionEnter::OBJECT[" + this.gameObject.name + "] COLLIDES WITH [" + _collision.collider.gameObject.name + "]");
            if (!EnableTriggerCollision) BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTOR_COLLISION_ENTER, this.gameObject, _collision.collider.gameObject);
        }


        // -------------------------------------------
        /* 
		 * OnTriggerEnter
		 */
        public virtual void OnTriggerEnter(Collider _collision)
        {
            // Debug.LogError("Actor::OnTriggerEnter::OBJECT[" + this.gameObject.name + "] TRIGGERS WITH [" + _collision.gameObject.name + "]");
            if (EnableTriggerCollision) BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTOR_COLLISION_ENTER, this.gameObject, _collision.gameObject);
        }

        // -------------------------------------------
        /* 
		 * OnCollisionExit
		 */
        public virtual void OnCollisionExit(Collision _collision)
        {
            // Debug.LogError("Actor::OnCollisionExit::OBJECT[" + this.gameObject.name + "] EXIT COLLIDES WITH [" + _collision.collider.gameObject.name + "]");
            if (!EnableTriggerCollision) BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTOR_COLLISION_EXIT, this.gameObject, _collision.collider.gameObject);
        }


        // -------------------------------------------
        /* 
		 * OnTriggerExit
		 */
        public virtual void OnTriggerExit(Collider _collision)
        {
            // Debug.LogError("Actor::OnTriggerExit::OBJECT[" + this.gameObject.name + "] EXIT TRIGGERS WITH [" + _collision.gameObject.name + "]");
            if (EnableTriggerCollision) BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTOR_COLLISION_EXIT, this.gameObject, _collision.gameObject);
        }

        // -------------------------------------------
        /* 
		 * OnNetworkEvent
		 */
        protected virtual void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
        {
#if ENABLE_MULTIPLAYER_TIMELINE
            if (_nameEvent == EVENT_ACTORTIMELINE_STATE_FACE_PLAYER)
            {
                string nameOrigin = (string)_list[0];
                if (nameOrigin == NameActor)
                {
                    bool behaviourFaceMainPlayer = bool.Parse((string)_list[1]);
                    if (behaviourFaceMainPlayer)
                    {
                        BaseObjectData targetBaseObjectData = GameLevelData.Instance.FindGameObject((string)_list[2]);
                        m_playerToFace = targetBaseObjectData.Character;
                    }
                    else
                    {
                        m_playerToFace = null;
                    }
                }
            }
#endif
            if (_nameEvent == EVENT_GAMEPLAYER_CREATED_NEW)
            {
                GameObject newPlayer = ((GameObject)_list[0]);
                if (newPlayer != this.gameObject)
                {
                    if (NetworkID!=null) NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA, NetworkID.GetID(), m_initialData);
                }
            }
            if ((_nameEvent == EVENT_GAMEPLAYER_HUMAN_DIRECTOR_NAME) || (_nameEvent == EVENT_GAMEPLAYER_HUMAN_SPECTATOR_NAME))
            {
                if (!DirectorMode)
                {
                    if (NetworkID != null) NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA, NetworkID.GetID(), m_initialData);
                }
            }
            if (_nameEvent == EVENT_GAMECHARACTER_NEW_ANIMATION)
            {
                if (!IsMine())
                {
                    if (GetModel() == null) return;

                    if (NetworkID != null)
                    {
                        if ((NetworkID.NetID == int.Parse((string)_list[0])) && (NetworkID.UID == int.Parse((string)_list[1])))
                        {
                            int newAnimation = int.Parse((string)_list[2]);
                            bool isLoopAnimation = bool.Parse((string)_list[3]);
                            base.ChangeAnimation(newAnimation, isLoopAnimation);
                            if (m_ghostPlayer != null)
                            {
                                ChangeAnimation(m_ghostPlayer.transform, newAnimation, isLoopAnimation, true);
                            }
                        }
                    }
                }
            }
            if (_nameEvent == EVENT_GAMECHARACTER_NEW_STATE)
            {
                if (!IsMine())
                {
                    if (NetworkID != null)
                    {
                        if ((NetworkID.NetID == int.Parse((string)_list[0])) && (NetworkID.UID == int.Parse((string)_list[1])))
                        {
                            int newState = int.Parse((string)_list[2]);
                            base.ChangeState(newState);
                        }
                    }
                }
            }
        }


        // -------------------------------------------
        /* 
		* Manager of global events
		*/
        protected virtual void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_GAMECHARACTER_POSITION_LOCAL_PLAYER)
            {
                m_positionLocalPlayer = (Vector3)_list[0];
            }
        }

        // -------------------------------------------
        /* 
        * GetClosestFreeNodeToCurrentPosition
        */
        public Vector3 GetClosestFreeNodeToCurrentPosition()
        {
            return PathFindingController.Instance.GetClosestFreeNode(this.transform.position);
        }

        // -------------------------------------------
        /* 
        * GoToPosition
        */
        public virtual bool GoToPosition(Vector3 _origin, Vector3 _target, float _speed, int _limitSearch, params string[] _masksToIgnore)
        {
#if ENABLE_MULTIPLAYER_TIMELINE
            m_goToPosition = true;
            m_speed = _speed;
            m_targetPath = Utilities.Clone(_target);            
            Utilities.Clone(ref m_masksToIgnore, _masksToIgnore);
            if (PathFindingController.Instance.CheckBlockedPath(_origin, _target, 3, m_masksToIgnore))
            {
                m_currentPathWaypoint = 0;
                CalculateInternalPathWaypoints(_origin, _target, _limitSearch, true, false);
                return (m_pathWaypoints.Count != 0);
            }
            else
            {
                m_currentPathWaypoint = -1;
                return true;
            }
#else
            return false;
#endif
            
        }

        // -------------------------------------------
        /* 
        * CalculateInternalPathWaypoints
        */
        protected virtual void CalculateInternalPathWaypoints(Vector3 _origin, Vector3 _target, int _limitSearch, bool _oneLayer = true, bool _raycastFilter = true)
        {
            m_pathWaypoints = new List<Vector3>();
            m_targetPath = Utilities.Clone(_target);
            if (PathFindingController.Instance.IsPrecalculated)
            {
                m_targetPath = PathFindingController.Instance.GetPath(_origin, m_targetPath, m_pathWaypoints, (_oneLayer?0:-1), _raycastFilter);
                m_pathWaypoints.Clear();
                m_pathWaypoints.Add(m_targetPath);
                /*
                GameObject dotReference = PathFindingController.Instance.CreateSingleDot(m_targetPath, 2, 0);
                GameObject.Destroy(dotReference, 3);
                */
                m_goToPosition = true;
            }
            else
            { 
                PathFindingController.Instance.GetPath(this.transform.position, m_targetPath, m_pathWaypoints, (_oneLayer ? 0 : -1), _raycastFilter, _limitSearch);
                if (m_pathWaypoints.Count > 0)
                {
                    m_currentPathWaypoint = 0;
                }
                else
                {
                    m_currentPathWaypoint = -1;
                    m_goToPosition = true;
                    if (Vector3.Distance(this.transform.position, _target) < DISTANCE_TO_UPDATE_TO_NEXT_WAYPOINT)
                    {
                        BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_ACTORTIMELINE_GO_ACTION_ENDED, 0.5f, NameActor, Name);
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
         * GoToTargetPosition
         */
        protected void GoToTargetPosition(bool _isMaster)
        {
            if (m_goToPosition && _isMaster)
            {
                if (PathFindingController.Instance.IsPrecalculated)
                {
                    LogicAlineation(m_targetPath, m_speed, 0.1f);

                    if (Utilities.DistanceXZ(this.transform.position, m_targetPath) < DISTANCE_TO_UPDATE_TO_NEXT_WAYPOINT)
                    {
                        m_goToPosition = false;
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTORTIMELINE_GO_ACTION_ENDED, NameActor, m_targetPath);
                    }
                }
                else
                {
                    // Debug.LogError("BaseObjectData::Logic::GO FROM["+ Character.transform.position.ToString() + "] TO POSITION::m_currentPathWaypoint=" + m_currentPathWaypoint + "::Speed["+ Character.GetComponent<Actor>().Speed + "]::target["+ m_targetPath.ToString() + "]");
                    if (m_currentPathWaypoint == -1)
                    {
                        LogicAlineation(m_targetPath, m_speed, 0.1f);
                    }
                    else
                    {
                        Vector3 subTarget = m_pathWaypoints[m_currentPathWaypoint];
                        LogicAlineation(subTarget, m_speed, 0.1f);
                        if (Utilities.DistanceXZ(this.transform.position, subTarget) < DISTANCE_TO_UPDATE_TO_NEXT_WAYPOINT)
                        {
                            m_currentPathWaypoint++;
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTORTIMELINE_WAYPOINT_UPDATED, NameActor, Name);
                            if (m_currentPathWaypoint >= m_pathWaypoints.Count)
                            {
                                m_currentPathWaypoint = -1;
                                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTORTIMELINE_GO_ACTION_ENDED, NameActor, Name);
                            }
                        }
                    }

                    if ((Utilities.DistanceXZ(this.transform.position, m_targetPath) < DISTANCE_TO_UPDATE_TO_NEXT_WAYPOINT) && (m_goToPosition))
                    {
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_ACTORTIMELINE_GO_ACTION_ENDED, NameActor, Name);
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		* OnTimelineEvent
		*/
        protected virtual void OnTimelineEvent(string _nameEvent, object[] _list)
        {
        }

        // -------------------------------------------
        /* 
		 * ChangeAnimation
		 */
        public override void ChangeAnimation(int _animation, bool _isLoop)
        {
            if (IsMine())
            {
                if (m_animation != _animation)
                {
                    if (NetworkID != null) NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_GAMECHARACTER_NEW_ANIMATION, 0.01f, NetworkID.NetID.ToString(), NetworkID.UID.ToString(), _animation.ToString(), _isLoop.ToString());
                }
                base.ChangeAnimation(_animation, _isLoop);
            }
        }

        // -------------------------------------------
        /* 
		 * ChangeState
		 */
        public override void ChangeState(int newState)
        {
            if (IsMine())
            {
                if (m_state != newState)
                {
                    if (NetworkID != null) NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_GAMECHARACTER_NEW_STATE, 0.01f, NetworkID.NetID.ToString(), NetworkID.UID.ToString(), newState.ToString());
                }
                base.ChangeState(newState);
            }
        }

        // ---------------------------------------------------
        /**
		 * InitGhostPlayer
		 */
        protected void InitGhostPlayer(string _classNamePrefab, Transform _root, Transform _model, Material _materialOnTop)
        {
#if !DISABLE_GHOST
            m_ghostPlayer = Utilities.AttachChild(_root, AssetbundleController.Instance.CreateGameObject(_classNamePrefab));
            Utilities.ApplyMaterialOnMeshes(m_ghostPlayer, _materialOnTop);
            m_ghostPlayer.transform.localScale = Utilities.Clone(_model.localScale);
            m_ghostPlayer.transform.localPosition = Utilities.Clone(_model.localPosition);
            m_ghostPlayer.SetActive(false);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
#endif
        }

        // ---------------------------------------------------
        /**
		 * GhostLogic
		 */
        protected void GhostLogic()
        {
            if (m_ghostPlayer != null)
            {
                Transform modelOtherPlayer = GetModel();
                if (modelOtherPlayer != null)
                {
                    float finalY = modelOtherPlayer.transform.localPosition.y;
                    if (Mathf.Abs(this.gameObject.transform.position.y - m_positionLocalPlayer.y) > DISTANCE_TO_ACTIVATE_GHOST)
                    {
                        if (!m_ghostPlayer.activeSelf) m_ghostPlayer.SetActive(true);
                        float finalPosition = m_positionLocalPlayer.y - (m_ghostPlayer.transform.localScale.y / 2);
                        m_ghostPlayer.transform.position = new Vector3(m_ghostPlayer.transform.position.x, finalPosition, m_ghostPlayer.transform.position.z);
                    }
                    else
                    {
                        if (m_ghostPlayer.activeSelf) m_ghostPlayer.SetActive(false);
                    }
                }
            }
        }

        // ---------------------------------------------------
        /**
		 * DispatchLocalEventPositionForGhost
		 */
        protected void DispatchLocalEventPositionForGhost()
        {
            m_timeForGhost += Time.deltaTime;
            if (m_timeForGhost > 0.2f)
            {
                m_timeForGhost = 0;
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_GAMECHARACTER_POSITION_LOCAL_PLAYER, this.gameObject.transform.position);
            }
        }

        // -------------------------------------------
        /* 
		 * Logic
		 */
        public override void Logic()
        {
            base.Logic();

            if (GetModel() == null) return;

#if ENABLE_MULTIPLAYER_TIMELINE
            if (m_playerToFace != null)
            {
                LogicAlineation(m_playerToFace.transform.position, 0, 0.2f);
            }
#endif
        }
    }
}
