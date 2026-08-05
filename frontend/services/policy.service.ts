import { api } from "./api";
import type { PolicyListResponse } from "@/types/policy";

export async function getPolicies() {
  return api<PolicyListResponse>("/nap/policies");
}

/* ===========================
   NAP API
   =========================== */

export async function getNapPolicies() {
  return api<any>("/nap/policies");
}

export async function getAllUserNap() {
  return api<any>("/nap/users");
}

export async function updateUserPolicy(
  uid: string,
  request: {
    policyId: string;
  },
) {
  return api(`/nap/users/${uid}/policy`, {
    method: "PUT",
    body: JSON.stringify(request),
  });
}