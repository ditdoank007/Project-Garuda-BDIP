import axios from "axios";

import type {
  Policy,
  PolicyFormData,
} from "@/types/policy";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

export async function createPolicy(
  policy: PolicyFormData,
) {
  const payload = {
    code: policy.code,
    name: policy.name,
    description: policy.description,
    isActive: policy.enabled,
  };

  const response = await axios.post(
    `${API_URL}/policies`,
    payload,
  );

  return response.data;
}

export async function updatePolicy(
  policy: Policy,
) {
  const payload = {
    id: policy.id,
    code: policy.code,
    name: policy.name,
    description: policy.description,
    isActive: policy.enabled,
    createdAt: policy.createdAt,
    updatedAt: policy.updatedAt,
  };

  const response = await axios.put(
    `${API_URL}/policies/${policy.id}`,
    payload,
  );

  return response.data;
}

export async function deletePolicy(
  id: string,
) {
  const response = await axios.delete(
    `${API_URL}/policies/${id}`,
  );

  return response.data;
}