# Interaction Substrates: Summary

## Basic Info
- **Title**: Interaction Substrates: Combining Power and Simplicity in Interactive Systems
- **Authors**: Wendy E. Mackay, Michel Beaudouin-Lafon
- **Venue**: CHI 2025

---

## Core Problem
Modern GUIs face a trade-off:
- Simple → limited power
- Powerful → too complex

Goal: **combine power and simplicity** via a new conceptual model.

---

## Core Idea: Substrates
A **substrate** is a *place for interaction*.

### Definition
A substrate:
1. Contains and structures objects
2. Manages constraints among objects
3. Maintains dependencies with other substrates or data sources

---

## Key Concepts

### 1. Objects of Interest
- Primary: content (shapes, text)
- Secondary: tools for control (e.g., styles)

### 2. Commands
- Actions applied to objects (menus, tools, etc.)

### 3. Power vs Simplicity
- **Simplicity** = effort to achieve result
- **Power** = scope of effect

---

## Key Mechanisms of Substrates

### 1. Structure
Substrates give meaning to objects.

Example:
- Same dots → different meaning in:
  - music score
  - graph
  - map

### 2. Constraints
- Persistent relationships between objects
- Replace repeated commands

Example:
- Spreadsheet formulas
- Alignment constraints

**Key idea: Reification of effects**

---

### 3. Dependencies
- Substrates depend on other substrates
- Changes propagate automatically

Example:
- Data → table → chart → display

---

### 4. Tweaking (Adjustment)
- Modify constraints without breaking them
- Persistent offsets

Example:
- Adjust alignment without removing constraint

---

### 5. Templating (Specialization)
- Turn structures into reusable templates
- Use placeholders

Example:
- Slide templates
- Spreadsheet formulas

---

## Extended Principles (from Instrumental Interaction)

| Principle | Command Side | Substrate Side |
|----------|-------------|---------------|
| Reification | Command → tool | Effect → constraint |
| Polymorphism | Works across objects | Constraints apply across types |
| Reuse | Reuse commands | Reuse structures |
| Adjustment | Tune parameters | Tweak constraints |
| Specialization | Curry tools | Create templates |

---

## Theoretical Foundations

- **Affordances**: context defines possible actions
- **Technical reasoning**: users infer system behavior
- **Naive physics**: users expect consistent rules
- **Co-adaptation**: users adapt system + system adapts user

---

## Key Contributions

1. Introduces **substrates as a new interaction abstraction**
2. Shifts focus from commands → **effects and relationships**
3. Provides **generative design principles**
4. Bridges gap between:
   - Programming power
   - Direct manipulation simplicity

---

## Design Implications

Good systems should:
- Make structure visible
- Support persistent relationships
- Allow customization (tweak + template)
- Enable multi-representation via dependencies

---

## Takeaway

Substrates enable:
> **“Simple tasks remain simple, complex tasks become possible.”**

They do this by:
- Turning actions into relationships
- Making systems predictable
- Giving users control over structure, not just commands
