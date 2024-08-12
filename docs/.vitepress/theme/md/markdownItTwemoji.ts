import type { PluginSimple } from "markdown-it";
import { twemoji } from './twemoji';

const markdownItTwemoji: PluginSimple = (md) => {
  md.core.ruler.after('markdownItMDinMD', 'markdownItTwemoji', (s) => s.src = twemoji.parse(s.src));
};

export default markdownItTwemoji;