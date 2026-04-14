import js from '@eslint/js'
import pluginQuery from '@tanstack/eslint-plugin-query'
import pluginImport from 'eslint-plugin-import'
import pluginPrettier from 'eslint-plugin-prettier/recommended'
import PluginReact from 'eslint-plugin-react'
import pluginReactDom from 'eslint-plugin-react-dom'
import pluginReactHooks from 'eslint-plugin-react-hooks'
import pluginReactRefresh from 'eslint-plugin-react-refresh'
import pluginReactX from 'eslint-plugin-react-x'
import { defineConfig, globalIgnores } from 'eslint/config'
import globals from 'globals'
import tseslint from 'typescript-eslint'

export default defineConfig([
  globalIgnores(['dist', '.yarn']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommendedTypeChecked,
      pluginImport.flatConfigs.recommended,
      {
        settings: {
          'import/resolver': {
            typescript: {
              project: ['./tsconfig.app.json', './tsconfig.node.json'],
              noWarnOnMultipleProjects: true,
            },
          },
        },
        rules: {
          'import/order': [
            'error',
            {
              groups: ['builtin', 'external', 'internal', 'parent', 'sibling', 'index', 'object'],
              'newlines-between': 'always',
              alphabetize: { order: 'asc', caseInsensitive: true },
            },
          ],
        },
      },
      pluginReactHooks.configs.flat.recommended,
      pluginReactRefresh.configs.vite,
      pluginPrettier,
      PluginReact.configs.flat.recommended,
      PluginReact.configs.flat['jsx-runtime'],
      pluginReactX.configs['recommended-typescript'],
      pluginReactDom.configs.recommended,
      ...pluginQuery.configs['flat/recommended'],
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
    },
    settings: {
      react: {
        version: 'detect',
      },
    },
    rules: {
      '@typescript-eslint/consistent-type-definitions': ['error', 'type'],
      '@typescript-eslint/no-floating-promises': 'off',
      '@typescript-eslint/prefer-promise-reject-errors': 'off',
      '@typescript-eslint/no-unused-vars': [
        'error',
        {
          argsIgnorePattern: '^_',
          varsIgnorePattern: '^_',
          // Allow unused parameters in type definitions
          args: 'after-used',
        },
      ],
      // Disable base rule as it conflicts with TypeScript version
      'no-unused-vars': 'off',
      // Sort React component props alphabetically
      'react/jsx-sort-props': [
        'error',
        {
          callbacksLast: true,
          shorthandFirst: true,
          ignoreCase: true,
          noSortAlphabetically: false,
          reservedFirst: true,
        },
      ],
    },
  },
])
