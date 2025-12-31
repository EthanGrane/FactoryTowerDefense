using System.Collections.Generic;
using UnityEngine;

public class ConveyorRender : MonoBehaviour
{
    // Clase usada para identificar de forma única cada slot visual de cada conveyor
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

    // Mapa que asocia cada slot lógico a su GameObject visual
    private Dictionary<ItemSlot, GameObject> activeItems = new Dictionary<ItemSlot, GameObject>();

    public GameObject itemPrefab;
    public int poolSize = 50;
    public Vector3 itemOffset = new Vector3(0, 1f, 0);

    // Pool para no instanciar constantemente
    private Queue<GameObject> pool = new Queue<GameObject>();


    void Awake()
    {
        // Se prepara el pool inicial de objetos visuales inactivos
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(itemPrefab);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }


    private void Start()
    {
        // Cada tick lógico del juego se actualizan las posiciones visuales
        LogicManager.Instance.OnTick += RenderConveyorItems;
    }


    void RenderConveyorItems()
    {
        // Lista de conveyors activos en el mundo
        ConveyorLogic[] conveyorBlocks = LogicManager.Instance.GetLogicByType<ConveyorLogic>();

        // Se registran los slots que siguen existiendo este tick
        HashSet<ItemSlot> stillActive = new HashSet<ItemSlot>();

        // Recorre todos los conveyors
        for (int i = 0; i < conveyorBlocks.Length; i++)
        {
            var conveyor = conveyorBlocks[i];

            // Dirección hacia la que empuja el conveyor según su rotación
            Vector2Int fwd = conveyor.ForwardFromRotation(conveyor.building.rotation);

            // Centro del tile del conveyor
            Vector3 basePos = new Vector3(
                conveyor.building.position.x + 0.5f,
                0,
                conveyor.building.position.y + 0.5f
            );
            
            basePos += itemOffset;

            // Punto inicial donde empieza el movimiento del item (ajustado ligeramente hacia atrás)
            Vector3 startPos = basePos - new Vector3(fwd.x, fwd.y, 0) * 0.6f;

            float slotDistance = 1f / ConveyorLogic.SLOT_COUNT;

            // Recorre los slots internos del conveyor
            for (int j = 0; j < conveyor.itemBuffer.Length; j++)
            {
                Item bufferItem = conveyor.itemBuffer[j];
                if (bufferItem == null) continue;

                // Slot único usado como llave del diccionario
                ItemSlot slot = new ItemSlot { conveyor = conveyor, slotIndex = j };
                stillActive.Add(slot);

                // Obtiene el objeto visual o crea uno del pool
                if (!activeItems.TryGetValue(slot, out GameObject visual))
                {
                    visual = pool.Count > 0 ? pool.Dequeue() : Instantiate(itemPrefab);
                    visual.SetActive(true);
                    activeItems[slot] = visual;
                }

                // Aplica el sprite del item
                visual.GetComponent<SpriteRenderer>().sprite = bufferItem.icon;

                // Avance del item dentro del conveyor (0 → inicio del slot, 1 → final)
                float progress = conveyor.GetItemProgressNormalized(j);

                // Se calcula el rango de movimiento de ese slot
                float slotStart = j * slotDistance;
                float slotEnd   = (j + 1) * slotDistance;

                // Posición interpolada dentro de ese rango
                float lerpedPosition = Mathf.Lerp(slotStart, slotEnd, progress);

                // Offset final en el espacio del conveyor
                Vector3 slotOffset = new Vector3(fwd.x, 0, fwd.y) * lerpedPosition;

                // Posición real donde se dibuja el item
                visual.transform.position = startPos + slotOffset;
            }
        }

        // Limpieza de items visuales que ya no están en ningún slot este tick
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

        // Eliminación real del diccionario
        foreach (var slot in toRemove)
            activeItems.Remove(slot);
    }

}
