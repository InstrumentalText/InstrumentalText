# Instrumental Interaction (Beaudouin-Lafon, CHI 2000)

## 1. Core Idea
Instrumental Interaction is an interaction model that generalizes **Direct Manipulation** by introducing **instruments (tools)** as mediators between users and objects.

- Traditional model: User → Object  
- Instrumental model: User → Instrument → Domain Object  

---

## 2. Key Concepts

### 2.1 Interaction Model
> A set of principles, rules, and properties guiding interface design.

---

### 2.2 Domain Objects
Objects that users actually care about in a task.

Examples:
- Text in a document
- Shapes in a graphics editor
- Data in a visualization

Properties:
- Have attributes (simple or complex)
- Can become objects of interest dynamically

---

### 2.3 Interaction Instruments
A mediator between user and domain object.

Structure:
- **User action → Instrument → Command → Domain Object → Feedback**

Key characteristics:
- Two-way transducer
- Provides:
  - Reaction (UI response)
  - Feedback (effect on object)

Examples:
- Scrollbar
- Selection handles
- Drawing tools

---

### 2.4 Activation of Instruments

Two types:

1. **Spatial activation**
   - Triggered by position (e.g., cursor over scrollbar)

2. **Temporal activation (mode)**
   - Triggered by previous action (e.g., selecting a tool)

Trade-off:
- Spatial → fast but occupies space
- Temporal → flexible but slower

---

### 2.5 Reification
Turning abstract concepts into manipulable objects.

Two types:
1. Concept → Domain Object (e.g., styles)
2. Command → Instrument (e.g., scrollbar)

---

### 2.6 Meta-Instruments
Instruments that operate on other instruments.

Examples:
- Toolbars
- Menus
- Tool palettes

---

## 3. Core Properties (Evaluation Metrics)

### 3.1 Degree of Indirection
Measures distance between user action and object response.

- Spatial offset (distance on screen)
- Temporal offset (delay in response)

Insight:
- Lower indirection ≈ more direct manipulation

---

### 3.2 Degree of Integration
Ratio between:
- Input DOF (device)
- Output DOF (task)

Example:
- Scrollbar (1D output / 2D input = 1/2)

Insight:
- Higher integration → more efficient interaction

---

### 3.3 Degree of Compatibility
Similarity between:
- User action
- System response

Examples:
- Dragging object → high compatibility
- Typing numbers → low compatibility

---

## 4. Critique of WIMP Interfaces

Problems:

1. **Indirect interaction**
   - Heavy reliance on menus/dialogs

2. **High indirection**
   - Spatial (dialogs far away)
   - Temporal (delayed feedback)

3. **Low integration**
   - Limited input devices

4. **Poor compatibility**
   - Text input for visual tasks

Conclusion:
WIMP violates principles of direct manipulation.

---

## 5. Post-WIMP Perspective

New interaction styles:
- Bimanual interaction
- Toolglasses
- Zoomable interfaces
- Augmented reality

Requirements for new model:
- Descriptive
- Comparative
- Generative

---

## 6. Design Example: Search Instrument

Traditional:
- Dialog-based
- Sequential interaction
- High temporal indirection

Instrumental design:
- Live highlighting of results
- Direct clicking to replace
- Non-modal interaction

Advantages:
- Immediate feedback
- Lower indirection
- More user control

---

## 7. Contributions

1. Unified model for WIMP and post-WIMP
2. Introduced **instrument-based abstraction**
3. Provided evaluation metrics:
   - Indirection
   - Integration
   - Compatibility
4. Demonstrated generative design (search tool)

---

## 8. Key Insight (Takeaway)

> Interaction is fundamentally **tool-mediated**, not object-direct.

Design implication:
- Don’t design commands  
- Design **instruments + relationships**

---

## 9. Relevance to Modern HCI / XR

Highly relevant to:
- VR/AR interaction design
- Multimodal interfaces
- Tool-based interaction systems
- Instrumental text / rule-based interaction

---

## 10. One-Sentence Summary

Instrumental Interaction reframes UI design as **users manipulating tools that act on objects**, enabling a more general, analyzable, and extensible interaction paradigm beyond WIMP.

