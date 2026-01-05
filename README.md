# Trivia Game/App (WIP)

A realtime, turn-based multiplayer trivia game built with **ASP.NET Core**, **SignalR**, and a **React + TypeScript** client.

This project is a **work in progress** and is shared early to showcase the architecture and core game loop rather than a finished product.

The goal is to create not just Yet Another Basic Trivia Game but a polished product that's actually fun to play. 

---

## Status

⚠️ **Under active development — not feature complete**

The realtime game loop is implemented and verified, but UI, persistence, and polish are still evolving.

---

## What Exists Already

### Backend
- ASP.NET Core Minimal APIs
- SignalR hub for realtime gameplay
- In-memory game store shared between REST and SignalR
- Server-authoritative, turn-based game loop
- Trivia questions sourced from a postgres database
- Validation for player turns, host-only actions, and answer submission

**REST**
- Create game
- Join game
- List active games
- CRUD for questions

**SignalR**
- Join game session
- Start game
- Submit answers
- Broadcast live game state updates

### Frontend
- React + TypeScript
- SignalR client
- Minimal UI used to verify the realtime game loop

---

## 🧠 Architecture

- REST API for basic request/response workflows
- SignalR (WebSockets) for realtime state synchronization
- Game state managed exclusively on the server
- Clients act as views; all rules are enforced server-side

## Why signal?
- SignalR is used instead of raw WebSockets because it offers many helpful abstractions for common features like reconnecting, group messaging and compatability. Most clients including web and native mobile have existing libraries for SignalR clients and I don't expect the app to have features that would need customization of raw WebSockets.


---

## Planned work not in any particular order or priority

- Improved UI and gameplay presentation
- More polished implementation for gameplay eg. reconnecting, persistence etc.
- MCP (Model Context Protocol) to make adding questions to database easier via Agentic AI
- More features for the game to set it apart from every other trivia game implementation.

---

## 🎯 Learning goals
- Building a server-authoritative game backend in C#/.NET
- Using WebSockets (via SignalR) for realtime multiplayer state
- Designing clean boundaries between REST and realtime APIs

## Running locally

Documentation for running the project locally will be added as the project stabilizes.
For now project requires separate commands for setting up database, compiling backend and building frontend so there's no simple tutorial to provide.
