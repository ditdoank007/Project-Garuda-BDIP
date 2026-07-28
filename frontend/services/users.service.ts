import { api } from "./api";
import type { UserListResponse } from "@/types/users";

export async function getUsers() {
  return api<UserListResponse>("/users");
}