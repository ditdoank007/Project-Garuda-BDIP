import axios from "axios";

import type {
  Location,
  LocationFormData,
} from "@/types/location";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function createLocation(
  location: LocationFormData,
) {
  const response = await axios.post(
    `${API_URL}/locations`,
    location,
  );

  return response.data;
}

export async function updateLocation(
  originalName: string,
  location: LocationFormData,
) {
  const response = await axios.put(
    `${API_URL}/locations/${encodeURIComponent(originalName)}`,
    location,
  );

  return response.data;
}

export async function deleteLocation(
  name: string,
) {
  const response = await axios.delete(
    `${API_URL}/locations/${encodeURIComponent(name)}`,
  );

  return response.data;
}