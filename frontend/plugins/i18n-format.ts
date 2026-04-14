import fs from 'fs'
import { dirname, resolve } from 'path'
import { fileURLToPath } from 'url'

const __filename = fileURLToPath(import.meta.url)
const __dirname = dirname(__filename)
const LOCALES_PATH = resolve(__dirname, '../public/locales/en.json')

function getNamespace(key: string): string {
  const dotIndex = key.indexOf('.')
  return dotIndex !== -1 ? key.substring(0, dotIndex) : key
}

function formatI18nFile(): void {
  // Read the JSON file
  const content = fs.readFileSync(LOCALES_PATH, 'utf-8')
  const translations: Record<string, string> = JSON.parse(content) as Record<string, string>

  // Get all keys and sort them alphabetically
  const sortedKeys = Object.keys(translations).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }))

  // Group keys by namespace
  const groupedByNamespace: Record<string, string[]> = {}
  for (const key of sortedKeys) {
    const namespace = getNamespace(key)
    if (!groupedByNamespace[namespace]) {
      groupedByNamespace[namespace] = []
    }
    groupedByNamespace[namespace].push(key)
  }

  // Get sorted namespaces
  const sortedNamespaces = Object.keys(groupedByNamespace).sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }))

  // Build the output JSON string with empty lines between namespaces
  const lines: string[] = ['{']

  sortedNamespaces.forEach((namespace, namespaceIndex) => {
    const keys = groupedByNamespace[namespace]

    keys.forEach((key, keyIndex) => {
      const value = translations[key]
      const escapedValue = JSON.stringify(value)
      const isLastKeyInNamespace = keyIndex === keys.length - 1
      const isLastNamespace = namespaceIndex === sortedNamespaces.length - 1
      const isLastKey = isLastNamespace && isLastKeyInNamespace

      const comma = isLastKey ? '' : ','
      lines.push(`  "${key}": ${escapedValue}${comma}`)
    })

    // Add empty line between namespaces (but not after the last one)
    if (!sortedNamespaces.indexOf(namespace) || namespaceIndex < sortedNamespaces.length - 1) {
      if (namespaceIndex < sortedNamespaces.length - 1) {
        lines.push('')
      }
    }
  })

  lines.push('}')

  // Write the formatted content back to the file
  const output = lines.join('\n') + '\n'
  fs.writeFileSync(LOCALES_PATH, output, 'utf-8')

  console.log('\n')
  console.log(`✓ Formatted ${LOCALES_PATH}`)
  console.log(`  - ${Object.keys(translations).length} keys sorted alphabetically`)
  console.log(`  - ${sortedNamespaces.length} namespaces: ${sortedNamespaces.join(', ')}`)
}

formatI18nFile()
