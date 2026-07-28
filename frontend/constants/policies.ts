import type { PolicyFormData } from "@/types/policy";

export const defaultPolicyForm: PolicyFormData = {
  code: "",
  name: "",
  description: "",
  priority: 100,
  enabled: true,
};
