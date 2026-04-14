import path from 'path'

import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

import { validateEnvPlugin } from './plugins/vite-env-plugin'
import { envSchema } from './src/types/EnvSchema'

// https://vite.dev/config/
export default defineConfig({
  plugins: [validateEnvPlugin(envSchema), react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
})
