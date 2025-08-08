import { defineConfig } from 'vitepress'
import { shared } from './shared'
import { ja } from './ja'
import { zh } from './zh'

export default defineConfig({
  ...shared,
  locales: {
    ja: { label: '日本語', ...ja },
    zh: { label: '简体中文', ...zh }
  }
})