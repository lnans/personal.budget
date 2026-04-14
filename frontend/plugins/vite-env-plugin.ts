import type { Plugin } from 'vite'
import { z } from 'zod'

// ANSI color codes
const gray = '\x1b[2m\x1b[90m' // Dim + gray for darker gray
const cyan = '\x1b[96m' // Bright cyan
const green = '\x1b[92m' // bright green
const red = '\x1b[91m' // Bright red
const reset = '\x1b[0m' // Reset

/**
 * Vite plugin to validate environment variables before starting dev server or building
 * Also convert primitive types (number, boolean) using zod's coercion
 * This ensures the application won't start with invalid configuration
 */
export function validateEnvPlugin(schema: z.ZodSchema): Plugin {
  return {
    name: 'validate-env',
    configResolved(config) {
      log(`validating environment variables for ${green}${config.mode}${reset} mode`)
      try {
        const result = schema.parse(config.env) as Record<string, unknown>
        Object.assign(config.env, result) // Update env with parsed values to keep type safety
        log(`${Object.keys(result).length} environment variables validated`)
      } catch (error) {
        if (error instanceof z.ZodError) {
          error.issues.map((issue) => logError(issue.message)).join('\n')
          process.exit(1)
        }
        throw error
      }
    },
  }
}

function log(message: string) {
  const time = new Date().toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    second: '2-digit',
    hour12: true,
  })

  console.log(`${gray}${time}${reset} ${cyan}[vite]${reset} ${gray}(env-plugin)${reset} ${message}`)
}

function logError(message: string) {
  log(`${red}${message}${reset}`)
}
