using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//[UpdateAfter(typeof(UnityEngine.Experimental.PlayerLoop.FixedUpdate))]
public class CharacterControllerDOTS : MonoBehaviour
{
   public float speed = 5.0f;
   public GameObject prefab; 
}

public struct PlayerTagComponent : IComponentData
{
}
public struct SpeedComponent : IComponentData
{
   public float Value;
}


public class PlayerCharacterBaker : Baker<CharacterControllerDOTS>
{
   public override void Bake(CharacterControllerDOTS authoring)
   {
      AddComponent(new PlayerTagComponent());
      AddComponent(new SpeedComponent {Value = authoring.speed});
   }
}
