import axios from "axios";

import type {
  Policy,
  PolicyFormData,
} from "@/types/policy";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function createPolicy(
  policy: PolicyFormData,
) {
  const response = await axios.post(
    `${API_URL}/nap/policies`,
    policy,
  );

  return response.data;
}

export async function updatePolicy(
  policy: Policy,
) {
  const response = await axios.put(
    `${API_URL}/nap/policies/${policy.id}`,
    policy,
  );

  return response.data;
}

export async function deletePolicy(
  id: string,
) {
  const response = await axios.delete(
    `${API_URL}/nap/policies/${id}`,
  );

  return response.data;
}