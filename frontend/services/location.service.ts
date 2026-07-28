import { api } from "./api";
import type { LocationListResponse } from "@/types/location";

export async function getLocations() {
  return api<LocationListResponse>("/locations");
}