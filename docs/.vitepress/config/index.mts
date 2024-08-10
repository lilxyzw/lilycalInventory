import { defineConfig } from 'vitepress'
import { shared } from './shared'
import { ja } from './ja'

export default defineConfig({
  ...shared,
  locales: {
    root: { label: '日本語', ...ja }
  }
})
