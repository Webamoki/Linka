
namespace Webamoki.Linka.Queries;

internal class ConditionQuery : Query
{
    private Query? _condition;
    protected Query Condition => _condition ??= new Query();

    public void SetCondition(BaseQuery condition, List<object> values)
    {
        Condition.ResetBody();
        Condition.AddBody(condition);
        Condition.AddValues(values);
    }

    public override bool IsEmpty() => base.IsEmpty() && Condition.IsEmpty();
    // private void AddCondition(string op, IQuery condition, string? value = null)
    // {
    //     var conditionQuery = Condition;
    //     if (!Condition.IsEmpty()) {
    //         Condition.AddBody(op);
    //     }
    //     if (condition is Query) {
    //         conditionQuery.AddBody("(", condition, ")");
    //     } else {
    //         conditionQuery.AddBody(condition);
    //     }
    //     if (value != null) {
    //         conditionQuery.AddValue(value);
    //     }
    // }
    //
    // public void OrCondition(IQuery condition, string? value = null)
    // {
    //     AddCondition("OR", condition, value);
    // }
    // public void  AndCondition(IQuery condition, string? value = null)
    // {
    //     AddCondition("AND", condition, value);
    // }




    // public void OrIn(AbstractModel|string $model, string $column, SelectQuery $query)
    // {
    //     $table = $model::TableName();
    //         $inQuery = new Query();
    //         $inQuery->addBody(
    //         "`$table`.`$column` IN (",
    //         $query,
    //     ')'
    //         );
    //     $this->orCondition($inQuery);
    // }
}



