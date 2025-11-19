// main.tsx
// -------------
// This is the entry point of the React application.
// It finds the <div id="root"> in index.html and mounts the React app there.

import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

// Import global styles for the entire application.
import "./index.css";

// Import the root React component which contains routing and layout.
import App from "./App";

// Find the root HTML element. The exclamation mark (!) tells TypeScript
// that we are sure "root" exists in the DOM.
const rootElement = document.getElementById("root")!;

// Create a React "root" and render our App inside StrictMode.
// StrictMode helps us catch potential problems during development.
createRoot(rootElement).render(
  <StrictMode>
    <App />
  </StrictMode>
);
