<template>
  <div>
    <h1>Data Visualization</h1>
    <div>
      <label>Country: </label>
      <select v-model="selectedCountry">
        <option value="">Select Country</option>
        <option
          v-for="country in countries"
          :key="country.code"
          :value="country.code"
        >
          {{ country.name }}
        </option>
      </select>
    </div>
    <div>
      <label>Subject: </label>
      <select v-model="selectedSubject">
        <option value="">Select Subject</option>
        <option
          v-for="subject in subjects"
          :key="subject.code"
          :value="subject.code"
        >
          {{ subject.name }}
        </option>
      </select>
    </div>
    <div v-if="isLoading" class="loading">Loading...</div>
    <Chart v-else :options="chartOptions"></Chart>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { Chart } from "highcharts-vue";
import { dataService } from "./services/dataService";

const selectedCountry = ref("");
const selectedSubject = ref("");

const isLoading = ref(true);

const [countries, subjects] = await Promise.all([
  dataService.getCountries(),
  dataService.getSubjects(),
]);

isLoading.value = false;

const chartOptions = ref({
  title: {
    text: "Data Chart",
  },
  xAxis: {
    categories: [] as string[],
  },
  series: [
    {
      type: "line",
      name: "Value",
      data: [] as number[],
    },
  ],
});

watch([selectedCountry, selectedSubject], async ([country, subject]) => {
  if (!country || !subject) {
    return;
  }
  isLoading.value = true;
  try {
    var data = await dataService.getChartData(country, subject);
    chartOptions.value.xAxis.categories = data.map((item) => item.year);
    chartOptions.value.series[0].data = data.map((item) => item.value);
  } finally {
    isLoading.value = false;
  }
});
</script>

<style scoped>
.loading {
  text-align: center;
  padding: 2rem;
  font-size: 1.2rem;
  color: #666;
}
</style>
