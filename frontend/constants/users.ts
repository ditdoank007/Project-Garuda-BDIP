/**
 * ================================================================
 * BDIP - Basarnas Digital Identity Platform
 * ================================================================
 *
 * File        : users.ts
 * Module      : Constants
 * Description : Default values for User module.
 * ================================================================
 */

import { UserFormData } from "@/types/users";

export const defaultUserForm: UserFormData = {
  username: "",
  fullName: "",
  email: "",
  unit: "",

  password: "",
  confirmPassword: "",

  enabled: true,
};