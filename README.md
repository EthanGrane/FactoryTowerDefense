# Estructura de Scripts – Deterministic Factory Game

## World / Tilemap System
- **World** → Singleton, contiene todos los Tiles
- **Tile** → Contiene referencia a Building, posición, tipo
- **TilemapRenderer** → Dibuja el mapa y las tiles en pantalla

## Building System
- **Building** → Clase base para todas las construcciones
  - ConveyorBlock
  - FactoryBlock
  - TurretBlock
- **BuildingLogic** → Lógica de comportamiento de cada building
  - ConveyorLogic
  - FactoryLogic
  - TurretLogic
- **BuildingRenderer** → Renderiza edificios, animaciones de belts/items

## Item / Inventory System
- **Item** → ScriptableObject, define tipo, sprite, stats
- **ItemStack** → Cantidad de item + referencia a Item
- **Inventory** → Contenedor de ItemStacks por Building

## Entity / Enemy System
- **Enemy** → Cada enemigo tiene su propio Update()
  - Position (float)
  - Speed
  - Path (lista de nodos)
  - Health
- **EnemyManager** → Lista de todos los enemigos, chunk management
- **PathfindingManager** → Calcula paths A* para enemigos

## Projectile / Weapon System
- **Projectile** → Posición float, velocidad, daño
- **BulletManager** → Pool de proyectiles, update y colisión simple

## Utilities / Managers
- **GameManager** → Tick, updates globales
- **PoolManager** → Pool de objetos (items, proyectiles)
- **InputManager** → Manejo de selección, hotkeys
