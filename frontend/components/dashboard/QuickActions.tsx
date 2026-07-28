import {
  UserPlus,
  UsersRound,
  RefreshCcw,
  DatabaseBackup,
} from "lucide-react";

const actions = [
  {
    title: "Add User",
    icon: UserPlus,
    color: "bg-blue-600",
  },
  {
    title: "Add Group",
    icon: UsersRound,
    color: "bg-green-600",
  },
  {
    title: "LDAP Sync",
    icon: RefreshCcw,
    color: "bg-orange-500",
  },
  {
    title: "Backup LDAP",
    icon: DatabaseBackup,
    color: "bg-violet-600",
  },
];

export default function QuickActions() {
  return (
    <div className="rounded-2xl bg-white border border-slate-200 shadow-sm p-6">

      <h2 className="text-xl font-bold">
        Quick Actions
      </h2>

      <p className="text-slate-500 mt-1">
        Frequently used administrative actions
      </p>

      <div className="mt-6 grid grid-cols-2 gap-4 lg:grid-cols-4">

        {actions.map((action) => {

          const Icon = action.icon;

          return (

            <button
              key={action.title}
              className="rounded-xl border border-slate-200 p-5 hover:shadow-lg transition hover:-translate-y-1"
            >

              <div
                className={`mx-auto flex h-12 w-12 items-center justify-center rounded-xl ${action.color}`}
              >
                <Icon className="text-white" size={22}/>
              </div>

              <p className="mt-4 font-semibold">
                {action.title}
              </p>

            </button>

          );

        })}

      </div>

    </div>
  );
}