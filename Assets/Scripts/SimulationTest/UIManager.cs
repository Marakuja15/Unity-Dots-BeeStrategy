using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;
using Unity.Entities;
using Unity.Mathematics;



public  class UIManager : MonoBehaviour
{
    [CreateProperty]
    public int numOfEntities { get; set; } = 0;
    private World _world;
    private EntityManager _entityManager;
    private EntityQuery _query;
    
  
    private int _uiNumOfEntities;
    private UIDocument _uiDocument;
    private VisualElement _root;
    
    void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        _root = _uiDocument.rootVisualElement;
        _root.dataSource = this;
        var addButton = _root.Q<Button>("add-button");
        var removeButton = _root.Q<Button>("remove-button");
        _world = World.DefaultGameObjectInjectionWorld;
        if(_world == null) return;
        _entityManager = _world.EntityManager;
        _query = _entityManager.CreateEntityQuery(typeof(SpawnerData));

      
        if (addButton != null)
        {
            addButton.clicked += OnAddClicked;
        }
        if (removeButton != null)
        {
            removeButton.clicked += OnRemoveClicked;
        }
     
    }

    void Update()
    {
        if (_root == null) return;
        
     
        float fps = 1.0f / Time.unscaledDeltaTime;
        
        var fpsLabel = _root.Q<Label>("fps-label");
        if (fpsLabel != null)
        {
            fpsLabel.text = "FPS: " + Mathf.RoundToInt(fps);
        }
    }
    private void OnRemoveClicked()
    {
        if (_query.HasSingleton<SpawnerData>())
        {
            Entity spawnerEntity = _query.GetSingletonEntity();
            SpawnerData spawnerData = _entityManager.GetComponentData<SpawnerData>(spawnerEntity);  
           
            int toSubstract = math.min(numOfEntities, _uiNumOfEntities);
            if(toSubstract <= 0) {
                _root.Q<Label>("entity-count-label").text = "Units: " + 0;
                return;
            };
            spawnerData.removeNumOfEntities += toSubstract;
            
            _entityManager.SetComponentData(spawnerEntity, spawnerData);
             _uiNumOfEntities -= toSubstract;
   
            _root.Q<Label>("entity-count-label").text = "Units: " + _uiNumOfEntities;
        }
    }
    private void OnAddClicked()
    {
    
        if(_query.HasSingleton<SpawnerData>())
        {
            Entity spawnerEntity = _query.GetSingletonEntity();
            SpawnerData spawnerData = _entityManager.GetComponentData<SpawnerData>(spawnerEntity);  
         
            spawnerData.numOfEntities += numOfEntities;
            _entityManager.SetComponentData(spawnerEntity, spawnerData);
            _uiNumOfEntities += numOfEntities;
            _root.Q<Label>("entity-count-label").text = "Units: " + _uiNumOfEntities;
        }

    }
    

}
