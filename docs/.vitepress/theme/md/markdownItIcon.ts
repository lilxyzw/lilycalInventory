import type { PluginSimple } from "markdown-it";

// LIICON()
const reg = /LIICON\((.+)\)/;

const markdownItIcon: PluginSimple = (md) => {
  const parse = (src) => {
    var cap;
    while(cap = reg.exec(src)) {
      src = src.substring(0, cap.index) + '<img class="emoji" draggable="false" src="/images/' + cap[1].trim() + '">' + src.substring(cap.index + cap[0].length);
    }
    return src;
  };

  md.core.ruler.after('normalize', 'markdownItIcon', (s) => s.src = parse(s.src));
};

export default markdownItIcon;