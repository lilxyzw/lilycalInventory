import { defineConfig } from 'vitepress'

const langName = '/ja';

export const ja = defineConfig({
  lang: 'ja_JP',
  description: "lilycalInventoryはコンポーネントを作れるだけで手軽にメニューを作れる魔法のインベントリーシステムです。",
  themeConfig: {
    logo: '/images/logo.svg',
    nav: [
      { text: 'ホーム', link: langName + '/' },
      { text: 'チュートリアル', link: langName + '/tutorial/', activeMatch: '/tutorial/' },
      { text: 'ドキュメント', link: langName + '/docs/', activeMatch: '/docs/' },
      { text: 'API', link: langName + '/api/', activeMatch: '/api/' }
    ],
    sidebar: [
      {
        text: 'チュートリアル',
        link: langName + '/tutorial/',
        collapsed: false,
        items: [
          { text: 'インストールの方法', link: langName + '/tutorial/install' },
          { text: 'オブジェクトのオンオフ', link: langName + '/tutorial/toggle' },
          { text: '衣装の切り替え', link: langName + '/tutorial/costume' },
          { text: 'アバターの明るさ調整', link: langName + '/tutorial/lightchanger' },
          { text: 'アバターの体型調整', link: langName + '/tutorial/morph' },
          { text: 'メニューの整理', link: langName + '/tutorial/menu' }
        ]
      },
      {
        text: 'ドキュメント',
        link: langName + '/docs/',
        collapsed: false,
        items: [
          {
            text: 'コンポーネント一覧', link: langName + '/docs/components',
            items: [
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_AutoDresser.png"> LI AutoDresser', link: langName + '/docs/components/autodresser' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_AutoDresserSettings.png"> LI AutoDresserSettings', link: langName + '/docs/components/autodressersettings' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_AutoFixMeshSettings.png"> LI AutoFixMeshSettings', link: langName + '/docs/components/autofixmeshsettings' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Comment.png"> LI Comment', link: langName + '/docs/components/comment' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_CostumeChanger.png"> LI CostumeChanger', link: langName + '/docs/components/costumechanger' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_ItemToggler.png"> LI ItemToggler', link: langName + '/docs/components/itemtoggler' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Material.png"> LI MaterialModifier', link: langName + '/docs/components/materialmodifier' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_MaterialOptimizer.png"> LI MaterialOptimizer', link: langName + '/docs/components/materialoptimizer' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_MenuFolder.png"> LI MenuFolder', link: langName + '/docs/components/menufolder' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Preset.png"> LI Preset', link: langName + '/docs/components/preset' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_Prop.png"> LI Prop', link: langName + '/docs/components/prop' },
              { text: '<img class="emoji" draggable="false" src="/lilycalInventory/images/LI_Script_SmoothChanger.png"> LI SmoothChanger', link: langName + '/docs/components/smoothchanger' },
            ]
          },
          { text: 'Direct Blend Treeを使用した最適化', link: langName + '/docs/directblendtree' },
          { text: 'NDMFや他ツールとの互換性', link: langName + '/docs/compatibility' },
          { text: '付属Prefabの説明', link: langName + '/docs/prefabs' }
        ]
      },
      {
        text: 'API',
        link: langName + '/api/',
        collapsed: false,
        items: [
          { text: 'API', link: langName + '/api/' }
        ]
      }
    ],
    search: {
      provider: 'local',
      options: {
        locales: {
          ja: {
            translations: {
              button: {
                buttonText: '検索',
                buttonAriaLabel: '検索'
              },
              modal: {
                noResultsText: '見つかりませんでした。',
                resetButtonTitle: '検索条件を削除',
                footer: {
                  selectText: '選択',
                  navigateText: '切り替え'
                }
              }
            }
          }
        }
      }
    },
    lastUpdated: {
      text: 'Updated at',
      formatOptions: {
        dateStyle: 'full',
        timeStyle: 'medium'
      }
    }
  }
})
