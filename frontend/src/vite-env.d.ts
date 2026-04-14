/**
 * Augment Vite's ImportMetaEnv interface with our environment variables
 * This provides type-safe access to import.meta.env in the application
 *
 * Using import() to reference the type without making this file a module
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type, @typescript-eslint/consistent-type-definitions
interface ImportMetaEnv extends Readonly<import('./types/EnvSchema').EnvSchemaType> {}

// eslint-disable-next-line @typescript-eslint/consistent-type-definitions -- interface merging for import.meta
interface ImportMeta {
  readonly env: ImportMetaEnv
}
