import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:5154',
        changeOrigin: true,
        secure: false, // fordi .NET bruker selvsignert sertifikat
      }
    }
  }
})


// This configuration sets up a development proxy between the React frontend and the ASP.NET Core backend.
// During development, the frontend (running on http://localhost:5173) sends any request starting with "/api"
// to the backend server (running on https://localhost:5154). 
// The `changeOrigin` and `secure: false` options ensure that HTTPS with a self-signed certificate works locally.
// This avoids CORS issues while developing, so the frontend can call the backend API seamlessly.

