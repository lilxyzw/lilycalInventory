<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter } from 'vitepress';

const router = useRouter();

onMounted(() => {
  const base = '/lilycalInventory/';
  const preferredLang = localStorage.getItem('preferredLang');
  const browserLang = navigator.language.startsWith('zh') ? 'zh' : 'ja';
  const targetLang = preferredLang || browserLang;

  router.go(base + targetLang + '/');
});
</script>

<template>
  <div>正在跳转...</div>
</template>