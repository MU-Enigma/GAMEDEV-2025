# 🐔 Hack-and-Slash Project Roadmap  

A structured step-by-step journey from learning **player movement** to creating a **shared hack-and-slash game** with the iconic **Blue Chicken** (or your own custom character).  

---

## 🔧 Unity Installation Tutorial  

Before starting, make sure everyone has Unity properly installed.  

### Step 1: Install Unity Hub  
1. Download **Unity Hub** → [Download Here](https://unity.com/download)  
2. Install and open Unity Hub.  

### Step 2: Install the Correct Unity Version  
1. In Unity Hub, go to **Installs** → **Install Editor**.  
2. Recommended version: **Unity 2022 LTS (Long-Term Support)**.  
3. Add **Modules** as needed (for Windows/Mac build support).  

### Step 3: Create Your First Project  
1. In Unity Hub → **Projects** → **New Project**.  
2. Template: **2D (URP optional if you want better graphics)**.  
3. Name your project → Place it in your personal folder (`/level-1`, `/level-2`, etc.).  
4. Hit **Create**. 🎉  

---

## 🎮 Level 1 — Movement Basics (Individual Folders)  

- Implement a **2D player controller** (movement with WASD/Arrow keys).  
- You may use the **Blue Chicken** from the doc (recommended) or your own sprite.  
- Push your project into your own folder.  

---

## 🗡️ Level 2 — First Weapon Swing (Individual Folders)  

- Add a **weapon swing** using the Animator component (e.g., sword slash).  
- Implement **collision detection** so the weapon can hit dummy targets/enemies.  
- Add **basic juice** (optional but encouraged):  
  - 📸 Screenshake when hitting  
  - ✨ Flash or particle when enemy is struck  
  - 🔊 Sound for the swing  
- Push in your own folder.  

---

## 🐓 Mini-Project — Weapon Showcase  

- Package your **character (chicken or custom) + weapon prefab**.  
- Add a **test scene** with dummy enemies to show off the weapon working.  
- Push into `/mini-projects`.  

---

## ⚔️ Level 3 — Shared Repo: Armory Build  

Everyone contributes to **one shared repo** where players can choose from multiple weapons.  

Each contributor adds **one unique weapon type**:  
- 🗡️ Sword (combo slash)  
- 🪓 Axe (wide knockback)  
- 🔱 Spear (pierce multiple enemies)  
- 🏹 Bow (projectiles)  
- ✨ Staff (AOE magic)  
- 🗡️ Dagger (fast multi-hits)  
- 🔨 Hammer (stun smash)  
- 🌀 Boomerang (returns to player)  
- 🔥 Flamethrower (crowd control)  
- 🐾 Claws/Fists (rapid melee)  

### Weapon Requirements:  
- Must feel **satisfying against groups of enemies**.  
- Preferably add **juice** (hit flashes, knockback, VFX, SFX).  

---
