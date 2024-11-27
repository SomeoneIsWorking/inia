<template>
  <Suspense>
    <template #default>
      <Chart></Chart>
    </template>
    <template #fallback>
      <div>Loading...</div>
    </template>
    <template #error="{ error }">
      <div>Error: {{ error.message }}</div>
    </template>
  </Suspense>
</template>

<script setup lang="ts">
import Chart from "./Chart.vue";
</script>
