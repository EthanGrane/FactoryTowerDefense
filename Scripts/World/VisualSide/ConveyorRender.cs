using System.Collections.Generic;
using UnityEngine;

public class ConveyorRender : MonoBehaviour
{
    // Identificador único para cada posición de item (conveyor específico + índice de slot)
    // Necesario porque múltiples conveyors pueden tener el mismo tipo de Item
    private class ItemSlot
    {
        public ConveyorLogic conveyor;
        public int slotIndex;

        public override bool Equals(object obj)
        {
            if (obj is ItemSlot other)
                return conveyor == other.conveyor && slotIndex == other.slotIndex;
            return false;
        }

        public override int GetHashCode()
        {
            return conveyor.GetHashCode() ^ slotIndex;
        }
    }

    // Diccionario que mapea cada slot a su GameObject visual
    private Dictionary<ItemSlot, GameObject> activeItems = new Dictionary<ItemSlot, GameObject>();
    public GameObject itemPrefab; // Prefab con SpriteRenderer para mostrar items
    public int poolSize = 50;
    private Queue<GameObject> pool = new Queue<GameObject>(); // Pool de objetos reutilizables

    void Awake()
    {
        // Inicializar pool de GameObjects
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(itemPrefab);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    private void Start()
    {
        // Suscribirse al evento de tick para actualizar visuales cada frame lógico
        LogicManager.Instance.OnTick += RenderConveyorItems;
    }

    void RenderConveyorItems()
    {
        ConveyorLogic[] conveyorBlocks = LogicManager.Instance.GetLogicByType<ConveyorLogic>();
        HashSet<ItemSlot> stillActive = new HashSet<ItemSlot>(); // Slots que siguen activos este frame

        // Iterar por todos los conveyors del mundo
        for (int i = 0; i < conveyorBlocks.Length; i++)
        {
            var conveyor = conveyorBlocks[i];
            
            // Calcular vectores de dirección según la rotación del conveyor
            Vector2Int fwd = conveyor.ForwardFromRotation(conveyor.building.rotation);

            // Centro del tile
            Vector3 basePos = new Vector3(
                conveyor.building.position.x + 0.5f,
                conveyor.building.position.y + 0.5f,
                0
            );

            // NUEVO: borde trasero del conveyor (corrige el +0.5f)
            Vector3 startPos = basePos - new Vector3(fwd.x, fwd.y, 0) * 0.6f;

            float slotDistance = 1f / ConveyorLogic.SLOT_COUNT;

            // Iterar por cada slot del buffer de items
            for (int j = 0; j < conveyor.itemBuffer.Length; j++)
            {
                Item bufferItem = conveyor.itemBuffer[j];
                if (bufferItem == null) continue;

                // Crear identificador único
                ItemSlot slot = new ItemSlot { conveyor = conveyor, slotIndex = j };
                stillActive.Add(slot);

                // Obtener / crear visual
                if (!activeItems.TryGetValue(slot, out GameObject visual))
                {
                    visual = pool.Count > 0 ? pool.Dequeue() : Instantiate(itemPrefab);
                    visual.SetActive(true);
                    activeItems[slot] = visual;
                }

                // Sprite del item
                visual.GetComponent<SpriteRenderer>().sprite = bufferItem.icon;

                // Progreso 0–1
                float progress = conveyor.GetItemProgressNormalized(j);
                float slotStart = j * slotDistance;
                float slotEnd   = (j + 1) * slotDistance;
                float lerpedPosition = Mathf.Lerp(slotStart, slotEnd, progress);

                // Offset correcto desde el borde trasero
                Vector3 slotOffset = new Vector3(fwd.x, fwd.y, 0) * lerpedPosition;

                visual.transform.position = startPos + slotOffset;
            }
        }

        // Limpiar visuales de slots que ya no existen
        List<ItemSlot> toRemove = new List<ItemSlot>();
        foreach (var kvp in activeItems)
        {
            if (!stillActive.Contains(kvp.Key))
            {
                kvp.Value.SetActive(false);
                pool.Enqueue(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var slot in toRemove)
            activeItems.Remove(slot);
    }

}