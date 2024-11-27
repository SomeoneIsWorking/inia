import { apiClient } from "../api/client";

export type ChartData = {
  year: string;
  value: number;
}

export type Country = {
  name: string;
  code: string;
}

export type Subject = {
  name: string;
  code: string;
}

export const dataService = {
  async getCountries(): Promise<Country[]> {
    const response = await apiClient.get<Country[]>("/data/countries");
    return response.data;
  },

  async getSubjects(): Promise<Subject[]> {
    const response = await apiClient.get<Subject[]>("/data/subjects");
    return response.data;
  },

  async getChartData(country: string, subject: string): Promise<ChartData[]> {
    const response = await apiClient.get<ChartData[]>(`/data`, {
      params: { country, subject },
    });
    return response.data;
  },
};
