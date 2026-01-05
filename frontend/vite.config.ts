import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
    build: {
    // backend expects the built files to be in this directory and it is easier to change it here than in the backend config
    outDir: path.resolve(__dirname, '../backend/wwwroot')
  }
})
