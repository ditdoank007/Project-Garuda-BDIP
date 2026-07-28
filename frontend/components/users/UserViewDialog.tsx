"use client";

import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog";

import { StatusBadge } from "@/components/common";

import { User } from "@/types/users";

interface UserViewDialogProps {

    open: boolean;

    onOpenChange: (open: boolean) => void;

    user: User | null;

}

export default function UserViewDialog({

    open,

    onOpenChange,

    user,

}: UserViewDialogProps) {

    if (!user) return null;

    return (

        <Dialog
            open={open}
            onOpenChange={onOpenChange}
        >

            <DialogContent className="sm:max-w-xl">

                <DialogHeader>

                    <DialogTitle>

                        User Information

                    </DialogTitle>

                </DialogHeader>

                <div className="grid grid-cols-2 gap-4 py-4">

                    <div className="font-medium">
                        Username
                    </div>

                    <div>
                        {user.username}
                    </div>

                    <div className="font-medium">
                        Full Name
                    </div>

                    <div>
                        {user.fullName}
                    </div>

                    <div className="font-medium">
                        Email
                    </div>

                    <div>
                        {user.email}
                    </div>

                    <div className="font-medium">
                        Unit
                    </div>

                    <div>
                        {user.unit || "-"}
                    </div>

                    <div className="font-medium">
                        Status
                    </div>

                    <div>

                        <StatusBadge
                            active={user.enabled}
                        />

                    </div>

                </div>

            </DialogContent>

        </Dialog>

    );

}