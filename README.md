# Unity DOTS Bee Strategy 

⚠️ **DISCLAIMER: This project is unfinished and is not a playable game.**  
This repository does not contain a complete gameplay loop or playable mechanics. It serves **exclusively as a code portfolio** to showcase my ability to design and implement highly optimized, multithreaded systems using Unity's Data-Oriented Technology Stack (DOTS), Entity Component System (ECS), the C# Job System, and the Burst Compiler.

---

## Recommended Code to Review
If you are reviewing my code, I highly recommend checking out the following systems, which demonstrate my approach to memory management, parallel processing, and entity state management:

### 1. `PollenCollectorSystem.cs`
**The core AI logic for resource gathering.** 
This system uses a custom spatial hashing implementation (`NativeParallelMultiHashMap`) to allow thousands of bees to efficiently query their surroundings for flowers. It handles distance calculations and safely assigns unique targets to worker bees using a `NativeHashSet` to prevent multi-booking, all executed in heavily optimized parallel jobs.

### 2. `BeeMovementSystem.cs`
**The foundational locomotion and steering system.**
Responsible for physically moving the massive swarm. It calculates velocities, updates `LocalTransform` components, and computes smooth quaternions (`quaternion.LookRotationSafe`) for thousands of entities simultaneously. It demonstrates how to perform high-density vector math cleanly inside Burst-compiled jobs without causing main-thread bottlenecks.

### 3. `FlowerPollenSystem.cs`
**Inter-entity interaction and resource extraction.**
This system manages the logic happening at the destination. It safely modifies both the flower's internal resource data and the bee's inventory (via `DynamicBuffer` and `ComponentLookup`). It showcases how I handle conditional component toggling (e.g., disabling the flower's availability once depleted) without breaking ECS structural changes.

### 4. `ReturnToHiveSystem.cs`
**State transitions and pathing home.**
Once a bee's inventory reaches maximum capacity, this system takes over. It queries for the nearest hive origin and transitions the bee's state components (disabling gathering tags and enabling movement/return tags). It's a great example of handling logical state machines natively within DOTS using `IEnableableComponent`.

---

## Tech Stack
* **Engine:** Unity 
* **Architecture:** Unity DOTS (Entities, Burst, C# Job System)
* **UI:** UI Toolkit (Note: UI assets are placeholders, focus is on logic & architecture)
