import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: 'dist',
    sourcemap: false,
    chunkSizeWarningLimit: 1000,
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('recharts') || id.includes('d3-')) return 'charts'
            if (id.includes('react-i18next') || id.includes('i18next')) return 'i18n'
            if (id.includes('react-dom') || id.includes('react-router')) return 'vendor'
            if (id.includes('@tanstack')) return 'query'
            return 'libs'
          }
        },
      },
    },
  },
  server: {
    port: 5173,
  },
})
