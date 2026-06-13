
function y = mu_law_normalize(x, mu)
    % MU_LAW_NORMALIZE Apply mu-law companding to EMG data.
    %
    % Inputs:
    %   x  - Input data (should be normalized to range [-1, 1])
    %   mu - Companding parameter (default: 255)
    %
    % Returns:
    %   y  - Companded data

    % Set default for mu if not provided
    if nargin < 2
        mu = 255;
    end

    % Use log1p (natural logarithm of 1 + x) for precision near zero
    % The dot (.) ensures element-wise operations for matrices/arrays
    numerator = log(1 + mu * abs(x));
    denominator = log(1 + mu);

    y = sign(x) .* (numerator / denominator);
end